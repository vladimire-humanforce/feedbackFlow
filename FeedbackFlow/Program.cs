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

            app.MapGet("/", () => "Hello from lambda!");
            app.MapGet("/ping", () => "pong");

            app.MapPost("/prompt", async ([FromServices] AmazonBedrockAgentRuntimeClient client, [FromBody] TextPromptRequest request) =>
            {
                var textModelId = request.Model switch
                {
                    LLModel.Nova_pro_v1 => "amazon.nova-pro-v1:0", //4
                    LLModel.Nova_lite_v1 => "amazon.nova-lite-v1:0", //5
                    LLModel.Nova_micro_v1 => "amazon.nova-micro-v1:0", //6
                    LLModel.Claude_3_haiku => "anthropic.claude-3-haiku-20240307-v1:0", //0, default
                    LLModel.Claude_3_sonnet => "anthropic.claude-3-sonnet-20240229-v1:0",//1
                    LLModel.Claude_3_5_sonnet_v1 => "anthropic.claude-3-5-sonnet-20240620-v1:0",//2
                    LLModel.Claude_3_5_sonnet_v2 => "anthropic.claude-3-5-sonnet-20241022-v2:0",//3
                    LLModel.Mistral_7b_instruct => "mistral.mistral-7b-instruct-v0:2",//7
                    _ => "anthropic.claude-3-haiku-20240307-v1:0"
                };
                const string knowledgeBaseId = "NBCQLJVBKO";
                var req = new RetrieveAndGenerateRequest
                {
                    Input = new RetrieveAndGenerateInput { Text = request.Prompt },
                    RetrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration
                    {
                        Type = RetrieveAndGenerateType.KNOWLEDGE_BASE,
                        KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration
                        {
                            KnowledgeBaseId = knowledgeBaseId,
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
