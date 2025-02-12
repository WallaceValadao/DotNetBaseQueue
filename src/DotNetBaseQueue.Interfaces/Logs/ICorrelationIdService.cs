namespace DotNetBaseQueue.Interfaces.Logs
{
    public interface ICorrelationIdService
    {
        string Get();
        void Set(string correlationId);
    }
}