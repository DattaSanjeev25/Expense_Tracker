# Deployment Guide

This guide provides instructions for deploying the Expense Tracker application.

## Prerequisites

- Node.js (v18 or later)
- .NET 9.0 SDK
- SQLite
- Angular CLI (v19)

## Backend Deployment

1. Navigate to the backend directory:
   ```bash
   cd ExpenseTracker.API
   ```

2. Update the `appsettings.json` file with production settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=ExpenseTracker.db"
     },
     "Jwt": {
       "SecretKey": "your-production-secret-key",
       "Issuer": "your-domain.com",
       "Audience": "your-domain.com",
       "ExpirationInMinutes": 60
     }
   }
   ```

3. Build the application:
   ```bash
   dotnet publish -c Release
   ```

4. Deploy the published files to your hosting environment:
   - Copy the contents of `bin/Release/net9.0/publish` to your web server
   - Configure your web server (IIS, Nginx, etc.) to serve the application
   - Set up SSL certificates for HTTPS

## Frontend Deployment

1. Navigate to the frontend directory:
   ```bash
   cd expense-tracker
   ```

2. Update the `environment.ts` file with production settings:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://your-api-domain.com'
   };
   ```

3. Build the application:
   ```bash
   ng build --configuration production
   ```

4. Deploy the built files:
   - Copy the contents of `dist/expense-tracker` to your web server
   - Configure your web server to serve the Angular application
   - Set up SSL certificates for HTTPS

## Database Setup

1. Run database migrations:
   ```bash
   cd ExpenseTracker.API
   dotnet ef database update
   ```

2. Backup the SQLite database file:
   ```bash
   cp ExpenseTracker.db ExpenseTracker.db.backup
   ```

## Security Considerations

1. Update the JWT secret key in production
2. Enable HTTPS for all endpoints
3. Configure CORS settings for your domain
4. Set up proper authentication and authorization
5. Implement rate limiting
6. Enable security headers

## Monitoring and Maintenance

1. Set up logging and monitoring
2. Configure error tracking
3. Set up automated backups
4. Monitor application performance
5. Keep dependencies updated

## Troubleshooting

1. Check application logs for errors
2. Verify database connectivity
3. Ensure all environment variables are set correctly
4. Check SSL certificate validity
5. Monitor server resources

## Rollback Procedure

1. Stop the application
2. Restore the database from backup
3. Deploy the previous version
4. Restart the application

## Support

For support and maintenance:
1. Monitor application logs
2. Set up alerts for critical errors
3. Maintain documentation
4. Keep contact information updated 