using Microsoft.Extensions.Logging;

namespace EnronEmailSearch.Core.ScaleCube
{
    /// <summary>
    /// X-axis: Horizontal Duplication (Cloning) implementation
    /// </summary>
    public class XAxisScaling
    {
        private readonly ILogger<XAxisScaling> _logger;
        private readonly XAxisConfiguration _configuration;
        
        public XAxisScaling(XAxisConfiguration configuration, ILogger<XAxisScaling> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        /// <summary>
        /// Generates an NGINX configuration for load balancing based on the X-axis configuration
        /// </summary>
        public string GenerateNginxConfig(string backendUrl, int startPort = 5000)
        {
            _logger.LogInformation("Generating NGINX config for {InstanceCount} instances", _configuration.InstanceCount);
            
            // Generate list of server entries
            List<string> serverEntries = new List<string>();
            for (int i = 0; i < _configuration.InstanceCount; i++)
            {
                int port = startPort + i;
                serverEntries.Add($"        server {backendUrl}:{port};");
            }
            
            // Set load balancing algorithm
            string algorithm = _configuration.LoadBalancingAlgorithm switch
            {
                LoadBalancingAlgorithm.LeastConnections => "least_conn",
                LoadBalancingAlgorithm.IPHash => "ip_hash",
                _ => "round_robin"
            };
            
            // Build NGINX configuration
            string loadBalancerDirective = algorithm != "round_robin" 
                ? $"        {algorithm};" 
                : "";
                
            string config = $@"

# NGINX configuration for X-axis scaling with {_configuration.InstanceCount} instances
# Generated automatically by Enron Email Search System

http {{
    upstream backend {{
{loadBalancerDirective}
{string.Join(Environment.NewLine, serverEntries)}
    }}
    
    server {{
        listen 80;
        
        location / {{
            proxy_pass http://backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }}
    }}
}}
";
            return config;
        }
    }
}