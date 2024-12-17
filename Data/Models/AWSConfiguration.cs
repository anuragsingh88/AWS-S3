namespace AWS_S3.Data.Models
{
    public class AWSConfiguration
    {
        public int ID { get; set; }
        public string Region { get; set; }
        public string Bucket { get; set; }
        public required string AccessKeyID { get; set; }
        public required string SecretAccessKey { get; set; }
    }
}
