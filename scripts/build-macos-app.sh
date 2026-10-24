#!/usr/bin/env bash
# Builds Chordious.app (the Avalonia front end) for macOS.
# Usage: scripts/build-macos-app.sh [osx-arm64|osx-x64]   (defaults to this Mac's architecture)
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RID="${1:-osx-$( [ "$(uname -m)" = "arm64" ] && echo arm64 || echo x64 )}"
PROJECT="$REPO_ROOT/src/Chordious.Avalonia/Chordious.Avalonia.csproj"
OUT="$REPO_ROOT/bld/Chordious.MacOS.$RID"
APP="$OUT/Chordious.app"

VERSION="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$REPO_ROOT/src/Directory.Build.props" | head -1)"

rm -rf "$OUT"
dotnet publish "$PROJECT" -c Release -r "$RID" --self-contained true -o "$OUT/publish"

mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$OUT/publish/." "$APP/Contents/MacOS/"

# App icon: convert the 128px PNG into an .icns
ICONSET="$OUT/Chordious.iconset"
mkdir -p "$ICONSET"
for size in 16 32 64 128; do
    sips -z $size $size "$REPO_ROOT/src/Chordious.Avalonia/Resources/chordious128.png" --out "$ICONSET/icon_${size}x${size}.png" > /dev/null
done
iconutil -c icns "$ICONSET" -o "$APP/Contents/Resources/Chordious.icns"
rm -rf "$ICONSET"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key><string>Chordious</string>
    <key>CFBundleDisplayName</key><string>Chordious</string>
    <key>CFBundleIdentifier</key><string>com.jonthysell.chordious</string>
    <key>CFBundleVersion</key><string>$VERSION</string>
    <key>CFBundleShortVersionString</key><string>$VERSION</string>
    <key>CFBundlePackageType</key><string>APPL</string>
    <key>CFBundleExecutable</key><string>Chordious</string>
    <key>CFBundleIconFile</key><string>Chordious.icns</string>
    <key>LSMinimumSystemVersion</key><string>11.0</string>
    <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
PLIST

# Ad-hoc sign so Gatekeeper lets it run locally (distribution would need a real identity + notarization)
codesign --force --deep --sign - "$APP" > /dev/null 2>&1 || echo "warning: ad-hoc codesign failed"

rm -rf "$OUT/publish"
echo "Built $APP"
