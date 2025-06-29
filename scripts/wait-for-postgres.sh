#!/bin/bash
# =================================================================
# PostgreSQL Readiness Wait Script
# Waits for PostgreSQL to be ready before starting the application
# =================================================================

set -e

host="$1"
port="$2"
shift 2
cmd="$@"

# Default values
if [ -z "$host" ]; then
    host="postgres"
fi

if [ -z "$port" ]; then
    port="5432"
fi

echo "🔄 PostgreSQL bekleniyor: $host:$port"

# Wait for PostgreSQL to become available
until pg_isready -h "$host" -p "$port" -U "postgres"; do
    echo "⏳ PostgreSQL henüz hazır değil - 2 saniye bekleniyor..."
    sleep 2
done

echo "✅ PostgreSQL hazır: $host:$port"

# If a command was provided, execute it
if [ ! -z "$cmd" ]; then
    echo "🚀 Komut çalıştırılıyor: $cmd"
    exec $cmd
fi 