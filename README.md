# MongoDBWatcher

1. Create an `appsettings.Development.json` file in the root directory if it does not exists.

2. Copy the content of `appsettings.json` and paste it to the `appsettings.Development.json` file.

3. add these settings if it does not exists.
```json
"DatabaseSettings": {
  "ConnectionString": "<mongodb connetctionstring>",
  "DatabaseName": "<database name>"
}
```

4/ Replace the `ConnectionString` and `DatabaseName` values with the ones you have.
