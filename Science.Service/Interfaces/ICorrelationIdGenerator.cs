namespace Science.Service.Interfaces
{
    public interface ICorrelationIdGenerator
    {
        string Get();
        void Set(string correlationId);
    }
}
