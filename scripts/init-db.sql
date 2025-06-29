-- =================================================================
-- PostgreSQL Database Initialization Script
-- ASP.NET Core Clean Architecture CQRS Template
-- =================================================================

-- Set timezone to UTC
SET timezone = 'UTC';

-- Create extensions if they don't exist
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Performance optimizations
SET shared_preload_libraries = 'pg_stat_statements';

-- Create additional databases if needed
-- (Main database TemplateDB is already created by environment variables)

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE "TemplateDB" TO postgres;

-- Performance indexes (will be applied after tables are created by EF Core)
-- These are just preparation comments for future use:
-- CREATE INDEX CONCURRENTLY idx_users_email ON "Users" USING btree ("Email");
-- CREATE INDEX CONCURRENTLY idx_users_username ON "Users" USING btree ("UserName");
-- CREATE INDEX CONCURRENTLY idx_refreshtokens_userid ON "RefreshTokens" USING btree ("AppUserId");
-- CREATE INDEX CONCURRENTLY idx_refreshtokens_token ON "RefreshTokens" USING btree ("Token");

-- Log table for Serilog (will be created automatically by Serilog, but we can prepare)
-- This is for the separate logging database

COMMENT ON DATABASE "TemplateDB" IS 'ASP.NET Core Clean Architecture CQRS Template Main Database';

-- Success message
DO $$
BEGIN
    RAISE NOTICE '✅ Database initialization completed successfully';
    RAISE NOTICE '📊 Database: TemplateDB';
    RAISE NOTICE '🚀 Ready for ASP.NET Core application';
END $$; 