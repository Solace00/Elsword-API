using Elsword_API.Services;

namespace Elsword_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Register DungeonScraper with HttpClient
            builder.Services.AddHttpClient<DungeonScraper>();

            // Register ElwikiScraper with HttpClient
            builder.Services.AddHttpClient<ElwikiInfoScraper>();

            // Register SkillScraper with HttpClient
            builder.Services.AddHttpClient<SkillListScraper>();
            builder.Services.AddSingleton<SkillListScraper>();
            builder.Services.AddHttpClient<SkillScraper>();
            builder.Services.AddScoped<SkillScraper>();



            // Add the mapping file paths to the configuration
            var dungeonsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "dungeons.json");

            // Add the JSON file paths to the configuration
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DungeonsFilePath", dungeonsFilePath }
            });

            // Register MappingService
            builder.Services.AddSingleton<MappingService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseCors("AllowAll");


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}