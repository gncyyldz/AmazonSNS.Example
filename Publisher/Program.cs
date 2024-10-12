using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Publisher.Notifications;
using Publisher.ViewModels;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

var app = builder.Build();

const string topicName = "product-topic";

app.MapPost("/create-product", async (CreateProductRequest createProductRequest, IAmazonSimpleNotificationService snsService) =>
{
    ProductCreatedNotification productCreatedNotification = new(
        createProductRequest.Id,
        createProductRequest.Name,
        createProductRequest.Description);

    //Topic Arn, SNS Topic'inin benzersiz kimli�ine kar��l�k gelmektedir.
    string topicArn = string.Empty;

    //Topic elde ediliyor.
    Topic topic = await snsService.FindTopicAsync(topicName);

    //Topic var m� yok mu kontrol ediliyor.
    if (topic is null)
    {
        //Topic yoksa olu�turuluyor.
        CreateTopicResponse createTopicResponse = await snsService.CreateTopicAsync(topicName);

        //Olu�turulan topic'in id'si elde ediliyor.
        topicArn = createTopicResponse.TopicArn;
    }
    else
        //Topic varsa id'si elde ediliyor.
        topicArn = topic.TopicArn;

    //Mesaj publish'i olu�turuluyor.
    PublishRequest publishRequest = new()
    {
        TopicArn = topicArn,
        Message = JsonSerializer.Serialize(productCreatedNotification),
        Subject = nameof(ProductCreatedNotification)
    };

    #region SNS Filter Policy
    publishRequest.MessageAttributes.Add("Scope", new MessageAttributeValue
    {
        DataType = "String",
        StringValue = "Email"
    });
    #endregion

    //Mesaj publish ediliyor.
    await snsService.PublishAsync(publishRequest);
});

app.Run();
