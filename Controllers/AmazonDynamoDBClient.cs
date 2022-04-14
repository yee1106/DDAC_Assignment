using Amazon;

namespace DDAC_Assignment.Controllers
{
    internal class AmazonDynamoDBClient
    {
        private string v1;
        private string v2;
        private string v3;
        private RegionEndpoint uSEast1;

        public AmazonDynamoDBClient(string v1, string v2, string v3, RegionEndpoint uSEast1)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.uSEast1 = uSEast1;
        }
    }
}