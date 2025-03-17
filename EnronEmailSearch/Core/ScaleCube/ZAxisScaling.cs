using Microsoft.Extensions.Logging;

namespace EnronEmailSearch.Core.ScaleCube
{
    /// <summary>
    /// Z-axis: Data Partitioning (Sharding) implementation
    /// </summary>
    public class ZAxisScaling
    {
        private readonly ILogger<ZAxisScaling> _logger;
        private readonly ZAxisConfiguration _configuration;

        public ZAxisScaling(ZAxisConfiguration configuration, ILogger<ZAxisScaling> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the shard ID for a given filename
        /// </summary>
        public int CalculateShardId(string filename)
        {
            if (!_configuration.Enabled)
                return 0;

            return _configuration.ShardingStrategy switch
            {
                ShardingStrategy.Range => CalculateRangeShardId(filename),
                ShardingStrategy.Hash => CalculateHashShardId(filename),
                ShardingStrategy.ModuloHash => CalculateModuloHashShardId(filename),
                ShardingStrategy.Directory => CalculateDirectoryShardId(filename),
                _ => 0
            };
        }

        /// <summary>
        /// Calculate shard ID based on range of first character
        /// </summary>
        private int CalculateRangeShardId(string filename)
        {
            string baseFileName = Path.GetFileName(filename).ToUpper();

            if (string.IsNullOrEmpty(baseFileName))
                return 0;

            char firstChar = baseFileName[0];

            // Calculate which range the first character falls into
            if (char.IsLetter(firstChar))
            {
                int alphabetSize = 26;
                int charsPerShard = (int)Math.Ceiling((double)alphabetSize / _configuration.ShardCount);
                int position = firstChar - 'A';
                return Math.Min(position / charsPerShard, _configuration.ShardCount - 1);
            }
            else if (char.IsDigit(firstChar))
            {
                // Put all numeric files in the last shard
                return _configuration.ShardCount - 1;
            }

            // Default to first shard for other characters
            return 0;
        }

        /// <summary>
        /// Calculate shard ID based on hash of filename
        /// </summary>
        private int CalculateHashShardId(string filename)
        {
            string baseFileName = Path.GetFileName(filename);

            if (string.IsNullOrEmpty(baseFileName))
                return 0;

            // Use simple hash function
            int hash = 0;
            foreach (char c in baseFileName)
            {
                hash = (hash * 31 + c) & 0x7FFFFFFF; // Keep positive
            }

            return hash % _configuration.ShardCount;
        }

        /// <summary>
        /// Calculate shard ID based on modulo hash of filename
        /// </summary>
        private int CalculateModuloHashShardId(string filename)
        {
            string baseFileName = Path.GetFileName(filename);

            if (string.IsNullOrEmpty(baseFileName))
                return 0;

            // Use GetHashCode for simplicity
            int hash = baseFileName.GetHashCode() & 0x7FFFFFFF; // Keep positive
            return hash % _configuration.ShardCount;
        }

        /// <summary>
        /// Calculate shard ID based on directory structure
        /// </summary>
        private int CalculateDirectoryShardId(string filename)
        {
            // Extract directory name
            string? directory = Path.GetDirectoryName(filename);

            if (string.IsNullOrEmpty(directory))
                return 0;

            // Use hash of directory path
            int hash = directory.GetHashCode() & 0x7FFFFFFF; // Keep positive
            return hash % _configuration.ShardCount;
        }

        /// <summary>
        /// Distributes files across shards for parallel processing
        /// </summary>
        public async Task<Dictionary<int, List<string>>> DistributeFilesAsync(
            IEnumerable<string> filePaths,
            CancellationToken cancellationToken = default)
        {
            var shardedFiles = new Dictionary<int, List<string>>();

            // Initialize shard buckets
            for (int i = 0; i < _configuration.ShardCount; i++)
            {
                shardedFiles[i] = new List<string>();
            }

            // Distribute files to shards
            foreach (var filePath in filePaths)
            {
                int shardId = CalculateShardId(filePath);
                shardedFiles[shardId].Add(filePath);

                // Check cancellation periodically
                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            // Log distribution statistics
            foreach (var shard in shardedFiles)
            {
                _logger.LogInformation("Shard {ShardId}: {FileCount} files", shard.Key, shard.Value.Count);
            }

            return shardedFiles;
        }
    }
}