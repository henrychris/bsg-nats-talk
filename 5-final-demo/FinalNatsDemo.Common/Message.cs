namespace FinalNatsDemo.Common
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
