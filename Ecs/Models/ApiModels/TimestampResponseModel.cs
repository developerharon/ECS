namespace ECS.Models.ApiModels
{
    public class TimestampResponseModel
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public bool IsClockActive { get; set; }
    }
}