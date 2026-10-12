# AgriTrade n8n v3 - Release Package

This package contains launchers for running AgriTrade on Windows and Android.

## Contents

- `start-agritrade.bat` - Windows launcher (double-click to start)
- `setup-android.sh` - Android setup script (requires ADB + QField)

## Requirements

### Windows
- [ ] Docker Desktop installed and running
- [ ] PostgreSQL 16.4 container (agritrade-pg)
- [ ] Node.js 18+ installed
- [ ] n8n installed globally (`npm install -g n8n`)
- [ ] PostgreSQL 18 client tools installed

### Android
- [ ] Android device with USB debugging enabled
- [ ] ADB installed on computer
- [ ] QField 4.3.3 installed on device

## Quick Start - Windows

1. Open Docker Desktop
2. Double-click `start-agritrade.bat`
3. Wait for services to start
4. Browser opens automatically at http://127.0.0.1:5678

## Quick Start - Android

1. Connect Android device via USB
2. Enable USB debugging
3. Run: `bash setup-android.sh`
4. Open QField on your device
5. Load the AgriTrade project

## Project Structure

```
AgriTrade_n8n_v3/
├── 01_پایگاه_داده/          # Database schema
├── 02_登记_فروشنده/          # Seller registration workflows
├── 03_登记_خریدار/           # Buyer registration workflows
├── 04_登记_عرضه/             # Offer workflows
├── 05_登记_تقاضا/            # Demand workflows
├── 06_تطبیق_عرضه_تقاضا/     # Matching workflows
├── 07_登记_المعامله/         # Transaction workflows
├── 08_登记_کمیسیون/          # Commission workflows
├── 09_گزارش/                 # Report workflows
├── 10_تلگرام/                # Telegram bot
├── 11_پنل_فارسی/             # Persian management panel
├── 12_پنل_فارسی_نسخه_۲/     # Persian panel v2
├── 13_فروش_حرفه‌ای_برگرفته_از_PDF/  # Professional sales (from PDF)
├── 14_منابع_PDF_فروش/        # PDF sales resources
├── shared/                   # Shared Python package
├── backend/                  # .NET 8 Web API
└── src/                      # Azure Functions
```

## Environment Variables

| Variable | Value | Description |
|----------|-------|-------------|
| N8N_HOST | 127.0.0.1 | n8n host |
| N8N_PORT | 5678 | n8n port |
| N8N_PROTOCOL | http | Protocol |
| DB_TYPE | postgresdb | Database type |
| DB_POSTGRESDB_HOST | 127.0.0.1 | PostgreSQL host |
| DB_POSTGRESDB_PORT | 5435 | PostgreSQL port |
| DB_POSTGRESDB_DATABASE | agritrade_db | Database name |
| DB_POSTGRESDB_USER | agritrade | Database user |
| N8N_RESTRICT_FILE_ACCESS_TO | F:\AgriTrade_n8n_v3_PDF_Integrated\input_files | File access restriction |
| NODE_FUNCTION_ALLOW_BUILTIN | * | Allow all built-in modules |

## License

Proprietary - AgriTrade Project