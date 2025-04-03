using Microsoft.AspNetCore.Mvc;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;

namespace FeedbackFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton(new AmazonBedrockAgentRuntimeClient());
            builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapPost("/prompt", async ([FromServices] AmazonBedrockAgentRuntimeClient client, [FromBody] TextPromptRequest request) =>
            {
                const string textModelId = "anthropic.claude-3-haiku-20240307-v1:0";
                const string knowledgeBaseId = "NBCQLJVBKO";
                var req = new RetrieveAndGenerateRequest
                {
                    Input = new RetrieveAndGenerateInput { Text = request.Prompt },
                    RetrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration
                    {
                        Type = RetrieveAndGenerateType.KNOWLEDGE_BASE,
                        KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration
                        {
                            KnowledgeBaseId = "NBCQLJVBKO",
                            ModelArn = textModelId,
                            RetrievalConfiguration = new KnowledgeBaseRetrievalConfiguration
                            {
                                VectorSearchConfiguration = new KnowledgeBaseVectorSearchConfiguration
                                {
                                    OverrideSearchType = "HYBRID"
                                }
                            }
                        }
                    }
                };
                var result = await client!.RetrieveAndGenerateAsync(req);
                var q = result.Output.Text!;
                return Results.Ok(q);
            })
            .WithName("Prompt")
            .WithOpenApi();

            app.Run();
        }
    }
}
