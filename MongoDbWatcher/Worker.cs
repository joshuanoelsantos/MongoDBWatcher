using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbWatcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DatabaseSettings _databaseSettings;

        public Worker(ILogger<Worker> logger, DatabaseSettings databaseSettings)
        {
            _logger = logger;
            _databaseSettings = databaseSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}

            //Execute();

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    Execute();
            //    await Task.Delay(1000, stoppingToken);
            //}

            //var client = new MongoClient("mongodb://127.0.0.1:27017/");
            //var db = client.GetDatabase("<databasename>");
            //var collection = db.GetCollection<TaskObject>("Task");

            //var results = collection
            //    .Find(new BsonDocument())
            //    .ToList();

            Execute();
        }

        private class TaskObject
        {
            [BsonId]
            public ObjectId Id { get; set; }

            [BsonElement("status")]
            public decimal Status { get; set; }

            [BsonElement("action")]
            public TaskAction Action { get; set; }

            [BsonElement("task_id")]
            public decimal TaskId { get; set; }

            [BsonElement("tm_id")]
            public ObjectId TmId { get; set; }

            [BsonElement("response")]
            public string Response { get; set; }
        }

        private class TaskAction
        {
            [BsonElement("intent")]
            public string Intent { get; set; }

            [BsonElement("type")]
            public string Type { get; set; }

            [BsonElement("params")]
            public string Params { get; set; }

            [BsonElement("method")]
            public string Method { get; set; }

            [BsonElement("url")]
            public string Url { get; set; }
        }

        private void Execute()
        {
            var cts = new CancellationTokenSource(5000);
            var client = new MongoClient(_databaseSettings.ConnectionString);
            //var db = client.GetDatabase(_databaseSettings.DatabaseName);
            var collection = client
                .GetDatabase(_databaseSettings.DatabaseName)
                .GetCollection<TaskObject>("Task");

            _logger.LogInformation("{time} - Start watching...", DateTimeOffset.Now);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TaskObject>>()
                .Match("{ operationType: { $in: [ 'replace', 'insert', 'update' ] } }");

            var cursor = collection.Watch(pipeline);

            try
            {
                while (cursor.MoveNext())
                {
                    if (cursor.Current.Count() > 0)
                    {
                        //using (var scope = _scopeFactory.CreateScope())
                        //{
                        //    var service = scope.ServiceProvider.GetRequiredService<TaskExecutionerService>();

                        foreach (var document in cursor.Current)
                        {
                            var task = document.FullDocument;
                            var type = document.OperationType;

                            //DbOperationType operationType;

                            //if (type == ChangeStreamOperationType.Insert)
                            //{
                            //    operationType = DbOperationType.Insert;
                            //}

                            //var service = new TaskExecutionerService();
                            //service.ExecuteAsync(task, operationType);
                        }
                        //}

                        _logger.LogInformation(
                            "{time} - Received {numberOfChanges} changes from database.",
                            DateTimeOffset.Now,
                            cursor.Current.Count());
                    }
                    else
                    {
                        _logger.LogInformation(
                            "{time} - Nothing changed in database.",
                            DateTimeOffset.Now);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "{time} - Exception: {errorMessage}",
                    DateTimeOffset.Now, e.Message);
            }
        }
    }
}