<?php
declare(strict_types=1);

$allowedOrigin = '*';

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: ' . $allowedOrigin);
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204);
    exit;
}

$dataFile = __DIR__ . '/highscore.json';

define('MAX_ENTRIES', 10);
define('MAX_SCORE', 6767);
define('MAX_ET', 90.0);
define('MAX_TRIES', 67);

function clampScore($n): int {
    if (!is_numeric($n)) return 0;
    $n = (int)$n;
    return max(0, min($n, MAX_SCORE));
}

function clampET($n): float {
    if (!is_numeric($n)) return MAX_ET;
    $n = (float)$n;
    return max(0.0, min($n, MAX_ET));
}

function clampTries($n): int {
    if (!is_numeric($n)) return MAX_TRIES;
    $n = (int)$n;
    return max(0, min($n, MAX_TRIES));
}

function sortHighScores(array &$list): void {
    usort($list, function($a, $b) {
        $sa = isset($a['score']) ? (int)$a['score'] : 0;
        $sb = isset($b['score']) ? (int)$b['score'] : 0;
        if ($sa !== $sb) return $sb <=> $sa;

        $ea = isset($a['et']) ? (float)$a['et'] : MAX_ET;
        $eb = isset($b['et']) ? (float)$b['et'] : MAX_ET;
        if ($ea !== $eb) return $ea <=> $eb;

        $ta = isset($a['tries']) ? (int)$a['tries'] : MAX_TRIES;
        $tb = isset($b['tries']) ? (int)$b['tries'] : MAX_TRIES;
        return $ta <=> $tb;
    });
}

function readHighScores(string $path): array {
    if (!file_exists($path)) {
        return [];
    }

    $raw = file_get_contents($path);
    if ($raw === false || $raw === '') {
        return [];
    }

    $json = json_decode($raw, true);
    if (!is_array($json) || !isset($json['highScores']) || !is_array($json['highScores'])) {
        return [];
    }

    $out = [];
    foreach ($json['highScores'] as $e) {
        if (!is_array($e)) continue;
        if (!isset($e['score']) || !is_numeric($e['score'])) continue;
        if (!isset($e['et']) || !is_numeric($e['et'])) continue;

        $out[] = [
            'score' => clampScore($e['score']),
            'et'    => clampET($e['et']),
            'tries' => clampTries($e['tries'])
        ];
    }

    return $out;
}

function writeHighScores(string $path, array $list): bool {
    $fp = fopen($path, 'c+');
    if (!$fp) return false;

    if (!flock($fp, LOCK_EX)) {
        fclose($fp);
        return false;
    }

    ftruncate($fp, 0);
    rewind($fp);

    $payload = ['highScores' => $list];
    $ok = fwrite($fp, json_encode($payload, JSON_PRETTY_PRINT)) !== false;
    fflush($fp);

    flock($fp, LOCK_UN);
    fclose($fp);

    return $ok;
}

function sendJson(array $payload, int $statusCode = 200): void {
    http_response_code($statusCode);
    echo json_encode($payload, JSON_PRETTY_PRINT);
    exit;
}

$method = $_SERVER['REQUEST_METHOD'];

if ($method === 'GET') {
    $list = readHighScores($dataFile);
    sortHighScores($list);
    $list = array_slice($list, 0, MAX_ENTRIES);

    sendJson([
        'highScores' => $list,
        'updated' => false
    ]);
}

if ($method === 'POST') {
    $rawBody = file_get_contents('php://input');
    $incoming = json_decode($rawBody, true);

    if (!is_array($incoming)) {
        sendJson(['error' => 'Invalid JSON body'], 400);
    }

    if (!isset($incoming['score']) || !is_numeric($incoming['score'])) {
        sendJson(['error' => "Missing numeric 'score'"], 400);
    }

    if (!isset($incoming['et']) || !is_numeric($incoming['et'])) {
        sendJson(['error' => "Missing numeric 'et'"], 400);
    }

    if (!isset($incoming['tries']) || !is_numeric($incoming['tries'])) {
        sendJson(['error' => "Missing numeric 'tries'"], 400);
    }

    $newScore = clampScore($incoming['score']);
    $newET = clampET($incoming['et']);
    $newTries = clampTries($incoming['tries']);

    $list = readHighScores($dataFile);
    sortHighScores($list);
    $list = array_slice($list, 0, MAX_ENTRIES);

    $updated = false;

    if (count($list) < MAX_ENTRIES) {
        $updated = true;
    } else {
        $last = $list[count($list) - 1];
        $lowestScore = (int)$last['score'];
        $lowestET = (float)$last['et'];
        $lowestTries = (int)$last['tries'];

        if ($newScore > $lowestScore) {
            $updated = true;
        }
        else if ($newScore === $lowestScore && $newET < $lowestET) {
            $updated = true;
        }
        else if ($newScore === $lowestScore && $newET === $lowestET && $newTries < $lowestTries) {
            $updated = true;
        }
    }

    if ($updated) {
        $list[] = [
            'score' => $newScore,
            'et'    => $newET,
            'tries' => $newTries
        ];

        sortHighScores($list);
        $list = array_slice($list, 0, MAX_ENTRIES);

        if (!writeHighScores($dataFile, $list)) {
            sendJson(['error' => 'Failed to write high scores'], 500);
        }
    }

    sortHighScores($list);
    $list = array_slice($list, 0, MAX_ENTRIES);

    sendJson([
        'highScores' => $list,
        'updated' => $updated
    ]);
}

sendJson(['error' => 'Method not allowed'], 405);