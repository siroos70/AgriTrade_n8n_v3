@echo off
REM ============================================
REM AgriTrade - Portable Launcher for Windows
REM ============================================
REM This script starts PostgreSQL, n8n, and opens the browser

setlocal enabledelayedexpansion

echo ============================================
echo   AgriTrade n8n v3 - Starting Services
echo ============================================
echo.

REM --- Step 1: Start PostgreSQL (Docker) ---
echo [1/4] Starting PostgreSQL...
docker start agritrade-pg >nul 2>&1
if %errorlevel% neq 0 (
    echo       Docker Desktop must be running first.
    echo       Launching Docker Desktop...
    start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    timeout /t 15 /nobreak >nul
    docker start agritrade-pg >nul 2>&1
)
echo       PostgreSQL container started.

REM --- Wait for PostgreSQL to be ready ---
echo [2/4] Waiting for PostgreSQL to accept connections...
set /a attempts=0
:wait_loop
set /a attempts+=1
if !attempts! gtr 15 goto :pg_timeout
set "PGPASSWORD=agritrade123"
"C:\Program Files\PostgreSQL\18\bin\psql.exe" -h 127.0.0.1 -p 5435 -U agritrade -d agritrade_db -tAc "SELECT 1;" >nul 2>&1
if %errorlevel% equ 0 goto :pg_ready
timeout /t 2 /nobreak >nul
goto wait_loop

:pg_timeout
echo       WARNING: PostgreSQL did not become ready in time.
echo       Continuing anyway - n8n will retry.

:pg_ready
echo       PostgreSQL is ready.

REM --- Step 3: Start n8n ---
echo [3/4] Starting n8n...
set "N8N_HOST=127.0.0.1"
set "N8N_PORT=5678"
set "N8N_PROTOCOL=http"
set "DB_TYPE=postgresdb"
set "DB_POSTGRESDB_HOST=127.0.0.1"
set "DB_POSTGRESDB_PORT=5435"
set "DB_POSTGRESDB_DATABASE=agritrade_db"
set "DB_POSTGRESDB_USER=agritrade"
set "DB_POSTGRESDB_PASSWORD=agritrade123"
set "N8N_AUTHENTICATION_KEY=n8n-proj-key-2026"
set "N8N_RESTRICT_FILE_ACCESS_TO=F:\AgriTrade_n8n_v3_PDF_Integrated\input_files"
set "NODE_FUNCTION_ALLOW_BUILTIN=*"

start /B node "C:\Users\siroo\AppData\Roaming\npm\node_modules\n8n\bin\n8n" start
echo       n8n starting in background...

REM --- Wait for n8n ---
echo [4/4] Waiting for n8n to be ready...
set /a attempts=0
:n8n_wait_loop
set /a attempts+=1
if !attempts! gtr 20 goto :n8n_timeout
curl -s -o nul -w "%{http_code}" "http://127.0.0.1:5678" >nul 2>&1
if %errorlevel% equ 0 goto :n8n_ready
timeout /t 3 /nobreak >nul
goto n8n_wait_loop

:n8n_timeout
echo       WARNING: n8n did not become ready.
echo       Opening browser anyway - it may take a moment.
goto open_browser

:n8n_ready
echo       n8n is ready!

:open_browser
echo.
echo ============================================
echo   AgriTrade is ready!
echo.
echo   URL: http://127.0.0.1:5678
echo.
echo   Keep this window open while using AgriTrade.
echo ============================================
echo.
timeout /t 5 /nobreak >nul
start "" "http://127.0.0.1:5678"