#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLAYER="$PROJECT_ROOT/Builds/Linux/MallDemo.x86_64"
OUTPUT="${1:-$PROJECT_ROOT/demo/MallDemo-demo.mp4}"
DURATION="${2:-30}"

if ! command -v ffmpeg >/dev/null 2>&1; then
    echo "ffmpeg is required to record the demo." >&2
    exit 1
fi
if ! command -v xdotool >/dev/null 2>&1; then
    echo "xdotool is required to locate the window." >&2
    exit 1
fi
if [[ ! -x "$PLAYER" ]]; then
    echo "Missing player: $PLAYER" >&2
    echo "Build it with ProjectBuilder.BuildStandalone first." >&2
    exit 1
fi

mkdir -p "$(dirname "$OUTPUT")"
LOG_FILE="${OUTPUT%.mp4}.log"

"$PLAYER" -screen-fullscreen 0 -screen-width 1024 -screen-height 768 -logFile "$LOG_FILE" >/dev/null 2>&1 &
PLAYER_PID=$!
cleanup() {
    kill "$PLAYER_PID" 2>/dev/null || true
    wait "$PLAYER_PID" 2>/dev/null || true
}
trap cleanup EXIT

WINDOW_ID=""
for _ in $(seq 1 80); do
    WINDOW_ID="$(xdotool search --onlyvisible --pid "$PLAYER_PID" 2>/dev/null | tail -1 || true)"
    if [[ -z "$WINDOW_ID" ]]; then
        WINDOW_ID="$(xdotool search --onlyvisible --name '^Mall Demo' 2>/dev/null | tail -1 || true)"
    fi
    [[ -n "$WINDOW_ID" ]] && break
    sleep 0.25
done
if [[ -z "$WINDOW_ID" ]]; then
    echo "Could not find the Mall Demo window." >&2
    exit 1
fi

xdotool windowmap "$WINDOW_ID" 2>/dev/null || true
xdotool windowraise "$WINDOW_ID"
xdotool windowactivate --sync "$WINDOW_ID"
xdotool windowsize "$WINDOW_ID" 1024 768
xdotool windowmove "$WINDOW_ID" 48 48
sleep 0.5

WINDOW_NAME="$(xdotool getwindowname "$WINDOW_ID")"
echo "Capturing $WINDOW_NAME"

GEOMETRY="$(xdotool getwindowgeometry "$WINDOW_ID")"
SIZE="$(printf '%s\n' "$GEOMETRY" | awk '/Geometry:/ {gsub("x", " ", $2); print $2, $3}')"
read -r WIDTH HEIGHT <<< "$SIZE"
if (( WIDTH > 1280 || HEIGHT > 900 )); then
    echo "Refusing to record a fullscreen-sized window: ${WIDTH}x${HEIGHT}" >&2
    exit 1
fi
echo "Recording window ${WINDOW_ID} (${WIDTH}x${HEIGHT})"

ffmpeg -y -loglevel error \
    -f x11grab -window_id "$WINDOW_ID" -framerate 30 -video_size "${WIDTH}x${HEIGHT}" -i :0.0 \
    -t "$DURATION" -c:v libx264 -preset veryfast -crf 22 -pix_fmt yuv420p \
    "$OUTPUT"

echo "Created $OUTPUT"
