#!/bin/bash
# ============================================
# AgriTrade - Android Launcher
# Requires: ADB, QField 4.3.3 installed on device
# ============================================

set -e

AGRI_TRADE_DIR="/storage/emulated/0/Android/data/ch.opengis.qfield/files/QField/plugins"
QFIELD_PLUGIN="agritrade-qfield-plugin.zip"

echo "============================================"
echo "  AgriTrade - Android Setup"
echo "============================================"
echo ""

# Step 1: Check ADB
echo "[1/5] Checking ADB..."
if ! command -v adb &> /dev/null; then
    echo "  ERROR: ADB not found. Install Android SDK Platform Tools."
    exit 1
fi
echo "  ADB found: $(adb --version | head -1)"

# Step 2: Check device
echo "[2/5] Checking Android device..."
DEVICES=$(adb devices | grep -c "device$")
if [ "$DEVICES" -eq 0 ]; then
    echo "  ERROR: No Android device connected."
    echo "  Connect your device and enable USB debugging."
    exit 1
fi
echo "  Device connected."

# Step 3: Check QField
echo "[3/5] Checking QField..."
QFIELD_PACKAGE=$(adb shell pm list packages | grep -o "ch.opengis.qfield" || true)
if [ -z "$QFIELD_PACKAGE" ]; then
    echo "  WARNING: QField not found on device."
    echo "  Install QField 4.3.3 from Google Play first."
    read -p "Continue anyway? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    QFIELD_VERSION=$(adb shell dumpsys package ch.opengis.qfield | grep versionName | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+')
    echo "  QField version: $QFIELD_VERSION"
fi

# Step 4: Push plugin
echo "[4/5] Pushing AgriTrade plugin..."
if [ -f "$QFIELD_PLUGIN" ]; then
    adb push "$QFIELD_PLUGIN" "$AGRI_TRADE_DIR/"
    echo "  Plugin pushed to: $AGRI_TRADE_DIR"
else
    echo "  WARNING: $QFIELD_PLUGIN not found. Skipping."
fi

# Step 5: Push project data
echo "[5/5] Pushing project data..."
PROJECT_DIR="/storage/emulated/0/AgriTrade"
adb shell mkdir -p "$PROJECT_DIR"
if [ -d "input_files" ]; then
    adb push input_files "$PROJECT_DIR/"
    echo "  Project data pushed."
fi

echo ""
echo "============================================"
echo "  Android setup complete!"
echo ""
echo "  Next steps:"
echo "  1. Open QField on your device"
echo "  2. Load the AgriTrade project"
echo "  3. Check layers and attributes"
echo "============================================"