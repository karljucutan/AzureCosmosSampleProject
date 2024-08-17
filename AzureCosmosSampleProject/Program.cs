using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace AzureCosmosSampleProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Create DB and Container
            //const string connectionString = "AccountEndpoint=https://klthcosmos.documents.azure.com:443/;AccountKey=4VhaduXbiIZZAMhCmKGT8H8eAMvuPBRbEI8To94QcwPA24GvRoYlkUBkbdw3g8PcODWy1gOEwn6tACDbcJ0Lgw==;";

            //CosmosClient cosmosClient = new CosmosClient(connectionString);

            //string databaseId = "appdb";

            //var database = await cosmosClient.CreateDatabaseAsync(databaseId);
            //Console.WriteLine("Azure Cosmos Db created");

            //string containerId = "courses";
            //string partitionKey = "/category";

            //await database.Database.CreateContainerAsync(containerId, partitionKey);
            //Console.WriteLine("Azure Cosmos Db Container created");

            // Create Courses
            //await CreateCourse(new Course("Az-104", 4.7, "Certification"));
            //await CreateCourse(new Course("Az-Kubernetes", 4.0, "Software"));
            //await CreateCourse(new Course("Az-204", 4.8, "Certification"));

            // Get and Display Courses
            var courses = await GetAndDisplayCourses();

            await UdateRating(courses.First(), 6);
        }

        private static CosmosClient ConnectDb()
        {
            const string connectionString = "AccountEndpoint=https://klthcosmos.documents.azure.com:443/;AccountKey=4VhaduXbiIZZAMhCmKGT8H8eAMvuPBRbEI8To94QcwPA24GvRoYlkUBkbdw3g8PcODWy1gOEwn6tACDbcJ0Lgw==;";

            return new CosmosClient(connectionString);
        }

        private static async Task CreateCourse(Course course) 
        {
            try
            {
                var cosmosClient = ConnectDb();
                string databaseId = "appdb";
                string containerId = "courses";

                var database = cosmosClient.GetDatabase(databaseId);
                var container = database.GetContainer(containerId);

                var item = await container.CreateItemAsync<Course>(course, new PartitionKey(course.category));
                Console.WriteLine(item);
            }
            catch (Exception ex) { }
        }

        private static async Task<List<Course>> GetAndDisplayCourses()
        {
            List<Course> courses = new List<Course>();
            var cosmosClient = ConnectDb();
            string databaseId = "appdb";
            string containerId = "courses";

            var database = cosmosClient.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            var queryDefinition = new QueryDefinition("SELECT * FROM courses");
            var feedIterator = container.GetItemQueryIterator<Course>(queryDefinition);
            using (feedIterator)
            {
                while (feedIterator.HasMoreResults) 
                { 
                    var response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        courses.Add(item);
                        Console.WriteLine($"Course Id: {item.id}");
                        Console.WriteLine($"Course Name: {item.name}");
                        Console.WriteLine($"Course Rating: {item.rating}");
                        Console.WriteLine($"Course Category: {item.category}");

                    }
                }
            }

            return courses;
        }

        private static async Task UdateRating(Course course, double newRating)
        {
            var cosmosClient = ConnectDb();
            string databaseId = "appdb";
            string containerId = "courses";

            var database = cosmosClient.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            await container.PatchItemAsync<Course>(
                course.id,
                new PartitionKey(course.category),
                new[] { PatchOperation.Replace("/rating", newRating) });

            Console.WriteLine($"Rating Updated: Id: {course.id}, Course: {course.name}, Category: {course.category}, Previous Rating: {course.rating}, New Rating: {newRating}");

        }
    }

    public class Course
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public double rating { get; set; }
        public string? category { get; set; }

        public Course()
        {
            
        }

        public Course(string? name, double rating, string? category)
        {
            id = Guid.NewGuid().ToString();
            this.name = name;
            this.rating = rating;
            this.category = category;
        }
    }
}
