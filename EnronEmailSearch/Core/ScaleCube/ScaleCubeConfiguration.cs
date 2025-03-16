namespace EnronEmailSearch.Core.ScaleCube
{
    /// <summary>
    /// Configuration for different Scale Cube dimensions
    /// </summary>
    public class ScaleCubeConfiguration
    {
        /// <summary>
        /// X-axis: Horizontal Duplication (Cloning)
        /// </summary>
        public XAxisConfiguration XAxis { get; set; } = new XAxisConfiguration();
        
        /// <summary>
        /// Y-axis: Functional Decomposition (Microservices)
        /// </summary>
        public YAxisConfiguration YAxis { get; set; } = new YAxisConfiguration();
        
        /// <summary>
        /// Z-axis: Data Partitioning (Sharding)
        /// </summary>
        public ZAxisConfiguration ZAxis { get; set; } = new ZAxisConfiguration();
    }
    
    /// <summary>
    /// X-axis: Horizontal Duplication (Cloning) configuration
    /// </summary>
    public class XAxisConfiguration
    {
        /// <summary>
        /// Number of instances to run
        /// </summary>
        public int InstanceCount { get; set; } = 2;
        
        /// <summary>
        /// Load balancing algorithm to use
        /// </summary>
        public LoadBalancingAlgorithm LoadBalancingAlgorithm { get; set; } = LoadBalancingAlgorithm.RoundRobin;
    }
    
    /// <summary>
    /// Y-axis: Functional Decomposition (Microservices) configuration
    /// </summary>
    public class YAxisConfiguration
    {
        /// <summary>
        /// Whether to enable microservice decomposition
        /// </summary>
        public bool Enabled { get; set; } = false;
        
        /// <summary>
        /// Microservices to enable
        /// </summary>
        public List<MicroserviceType> EnabledServices { get; set; } = new List<MicroserviceType>
        {
            MicroserviceType.CleanerService,
            MicroserviceType.IndexerService,
            MicroserviceType.SearchService,
            MicroserviceType.WebService
        };
    }
    
    /// <summary>
    /// Z-axis: Data Partitioning (Sharding) configuration
    /// </summary>
    public class ZAxisConfiguration
    {
        /// <summary>
        /// Whether to enable sharding
        /// </summary>
        public bool Enabled { get; set; } = false;
        
        /// <summary>
        /// Number of shards to use
        /// </summary>
        public int ShardCount { get; set; } = 4;
        
        /// <summary>
        /// Sharding strategy to use
        /// </summary>
        public ShardingStrategy ShardingStrategy { get; set; } = ShardingStrategy.Range;
    }
    
    /// <summary>
    /// Load balancing algorithms
    /// </summary>
    public enum LoadBalancingAlgorithm
    {
        RoundRobin,
        LeastConnections,
        IPHash
    }
    
    /// <summary>
    /// Types of microservices
    /// </summary>
    public enum MicroserviceType
    {
        CleanerService,
        IndexerService,
        SearchService,
        WebService
    }
    
    /// <summary>
    /// Sharding strategies
    /// </summary>
    public enum ShardingStrategy
    {
        Range,       // Partition by range (e.g., A-F, G-M, N-S, T-Z)
        Hash,        // Partition by hash of the filename
        ModuloHash,  // Partition by hash mod shard count
        Directory    // Partition by directory structure
    }
}