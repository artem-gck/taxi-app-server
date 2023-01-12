namespace Contracts.Shared
{
    public interface ISetWaitingUserStatusRequest
    {
        public Guid UserId { get; set; }
    }
}