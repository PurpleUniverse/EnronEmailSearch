workspace {
    name "Enron Email Search System"
    description "A system for cleaning, indexing, and searching the Enron email dataset"

    model {
        # People
        user = person "User" "A person who wants to search through the Enron email dataset"
        
        # Software Systems
        emailSearchSystem = softwareSystem "Enron Email Search System" "Allows users to clean, index, and search emails from the Enron dataset" {
            # Containers
            webApplication = container "Web Application" "Provides web interface for searching emails and downloading files" "ASP.NET Core MVC" {
                searchController = component "Search Controller" "Handles search requests and results display" "ASP.NET Core MVC Controller"
                searchResultView = component "Search Result View" "Displays search results to users" "Razor View"
                fileDownloadComponent = component "File Download Component" "Allows downloading original email files" "ASP.NET Core MVC Controller"
                emailPreviewComponent = component "Email Preview Component" "Displays preview of email content" "Razor View"
            }
            
            indexerApplication = container "Indexer Application" "Console application for cleaning and indexing emails" "Console Application" {
                commandLineInterface = component "Command Line Interface" "Parses command line arguments and runs operations" ".NET Console"
                cleaningOrchestrator = component "Cleaning Orchestrator" "Coordinates the email cleaning process" ".NET Core"
                indexingOrchestrator = component "Indexing Orchestrator" "Coordinates the email indexing process" ".NET Core"
            }
            
            coreLibrary = container "Core Library" "Contains core domain logic and interfaces" ".NET Class Library" {
                domainModels = component "Domain Models" "Core domain model classes" "C# Classes"
                emailCleanerService = component "Email Cleaner Service" "Service for cleaning email content" "C# Service"
                emailIndexerService = component "Email Indexer Service" "Service for indexing email content" "C# Service"
                searchService = component "Search Service" "Service for searching indexed emails" "C# Service"
                resilientServices = component "Resilient Services" "Fault tolerance wrappers around core services" "C# Decorators with Polly"
                interfaces = component "Service Interfaces" "Interfaces for dependency injection" "C# Interfaces"
            }
            
            infrastructureLibrary = container "Infrastructure Library" "Contains data access and external dependencies" ".NET Class Library" {
                dbContext = component "Database Context" "Entity Framework Core DB context" "Entity Framework Core"
                resilienceService = component "Resilience Service" "Implements resilience patterns" "Polly"
                repositories = component "Repositories" "Data access repositories" "C# Classes"
            }
            
            database = container "SQLite Database" "Stores indexed emails and search terms" "SQLite" {
                wordsTable = component "Words Table" "Stores unique words" "SQLite Table"
                filesTable = component "Files Table" "Stores email file information and content" "SQLite Table"
                occurrencesTable = component "Occurrences Table" "Maps relationships between words and files" "SQLite Table"
            }
            
            monitoringSystem = container "Monitoring System" "Collects metrics and provides dashboards" "Prometheus + Grafana" {
                prometheusComponent = component "Prometheus" "Collects and stores metrics" "Prometheus"
                grafanaComponent = component "Grafana" "Visualizes metrics in dashboards" "Grafana"
                healthChecks = component "Health Checks" "Provides application health status" "ASP.NET Core Health Checks"
            }
        }
        
        # External Systems
        fileSystem = softwareSystem "File System" "Stores raw and cleaned email files" "External"
        loadBalancer = softwareSystem "NGINX Load Balancer" "Distributes traffic across application instances" "NGINX"
        
        # Relationships between people and systems
        user -> loadBalancer "Accesses via web browser"
        loadBalancer -> emailSearchSystem "Routes requests to"
        
        # Relationships between containers
        loadBalancer -> webApplication "Routes requests to"
        webApplication -> coreLibrary "Uses"
        webApplication -> infrastructureLibrary "Uses"
        webApplication -> database "Reads from"
        
        indexerApplication -> coreLibrary "Uses"
        indexerApplication -> infrastructureLibrary "Uses"
        indexerApplication -> fileSystem "Reads from and writes to"
        indexerApplication -> database "Writes to"
        
        coreLibrary -> infrastructureLibrary "Uses"
        infrastructureLibrary -> database "Reads from and writes to"
        
        webApplication -> monitoringSystem "Provides metrics to"
        indexerApplication -> monitoringSystem "Provides metrics to"
        
        # Relationships between components
        searchController -> interfaces "Uses"
        searchController -> searchResultView "Uses"
        searchController -> fileDownloadComponent "Uses"
        searchController -> emailPreviewComponent "Uses"
        
        commandLineInterface -> cleaningOrchestrator "Executes"
        commandLineInterface -> indexingOrchestrator "Executes"
        
        cleaningOrchestrator -> emailCleanerService "Uses"
        indexingOrchestrator -> emailIndexerService "Uses"
        
        emailCleanerService -> interfaces "Implements"
        emailIndexerService -> interfaces "Implements"
        searchService -> interfaces "Implements"
        
        emailIndexerService -> dbContext "Uses"
        searchService -> dbContext "Uses"
        
        resilientServices -> emailCleanerService "Decorates"
        resilientServices -> emailIndexerService "Decorates"
        resilientServices -> searchService "Decorates"
        resilientServices -> resilienceService "Uses"
        
        dbContext -> wordsTable "Manages"
        dbContext -> filesTable "Manages"
        dbContext -> occurrencesTable "Manages"
        
        repositories -> dbContext "Uses"
    }

    views {
        systemContext emailSearchSystem "SystemContext" {
            include *
            autoLayout
        }
        
        container emailSearchSystem "Containers" {
            include *
            autoLayout
        }
        
        component webApplication "WebComponents" {
            include *
            autoLayout
        }
        
        component coreLibrary "CoreComponents" {
            include *
            autoLayout
        }
        
        component infrastructureLibrary "InfrastructureComponents" {
            include *
            autoLayout
        }
        
        component database "DatabaseComponents" {
            include *
            autoLayout
        }
        
        component monitoringSystem "MonitoringComponents" {
            include *
            autoLayout
        }
        
        theme default
    }
}