using Amazon.Lambda.Core;

namespace MinimalApiWithLambda.Api.Lambda;

public sealed class LambdaContextStub : ILambdaContext
{
    public string AwsRequestId => Guid.NewGuid().ToString("N");
    public IClientContext ClientContext => null!;
    public string FunctionName => "MinimalApiAdapter";
    public string FunctionVersion => "1";
    public ICognitoIdentity Identity => null!;
    public string InvokedFunctionArn => "local";
    public ILambdaLogger Logger { get; } = new ConsoleLambdaLogger();
    public string LogGroupName => "local";
    public string LogStreamName => "local";
    public int MemoryLimitInMB => 256;
    public TimeSpan RemainingTime => TimeSpan.FromMinutes(1);

    private sealed class ConsoleLambdaLogger : ILambdaLogger
    {
        public void Log(string message) => Console.Write(message);
        public void LogLine(string message) => Console.WriteLine(message);
    }
}
