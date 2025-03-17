# Enron Email Search System

A scalable, resilient system for cleaning, indexing, and searching the Enron email dataset. This project demonstrates advanced software architecture principles including the Scale Cube, fault isolation, and comprehensive monitoring.

## Project Overview

The Enron Email Search System processes the publicly available Enron email dataset (approximately 1.7GB) and makes it searchable through a web interface. The system is designed with three main components:

1. **Email Cleaner**: Removes headers from raw email files and prepares them for indexing
2. **Email Indexer**: Creates a searchable index in a SQLite database
3. **Search Interface**: Provides a web UI for searching and viewing emails

The architecture follows clean architecture principles with separate Core, Infrastructure, and Presentation layers.

## Key Features

- **Scalable Architecture**: Implementation of all three Scale Cube dimensions (X, Y, Z axes)
- **Fault Tolerance**: Comprehensive resilience patterns using Polly
- **Containerization**: Docker and docker-compose for easy deployment
- **Monitoring**: Prometheus and Grafana for metrics and visualization
- **Performance**: Optimized for speed in both indexing and searching

## System Architecture

### C4 Model

The system is designed using the C4 model approach with clear definitions of:
- **Context**: System boundaries and external interactions
- **Containers**: Major building blocks like web application, indexer, and database
- **Components**: Internal components of each container
- **Code**: Implementation details

### Database Schema

We evolved the database schema from the one provided throught the assignment. Here's a text representation of the entity relationships:

![dL9TgzD047tVNp7S5q7C7v3BfQ8jI55BM_11VDXkndPnFvRTYHfQ_xkPNTk49b6uRvDpDkUS-LWaaf4QQyGBM3hIr5OHZIJjNJhf0BMMnoWjVFEnuwDtiBHI6zYXZEe4kpOHMf6-QnSalpFQ8RK1mKSwUI4mQyB3Pn_h1v09VdO5d0N0IuRUr_0Qj2DuVDoidvuwkAe4m1v_KKSaYjeT](https://github.com/user-attachments/assets/7cb9d559-01e0-41ef-9368-7c9fb2b0a6fd)



This schema design provides:

#### Core Entities

- **Word**: Stores unique terms found in the dataset
  - `WordId` (PK): Unique identifier
  - `Text`: The actual word/term

- **EmailFile**: Stores information about email files
  - `FileId` (PK): Unique identifier
  - `FileName`: Original file name
  - `Content`: Raw email content

- **Occurrence**: Junction table tracking word occurrences in files
  - `WordId` (PK, FK): Reference to Word
  - `FileId` (PK, FK): Reference to EmailFile
  - `Count`: Number of occurrences of the word in the file

#### Extended Entities

- **Contact**: Email senders and recipients
  - `ContactId` (PK): Unique identifier
  - `EmailAddress`: Email address
  - `Name`: Contact name
  - `Company`: Company affiliation
  - `Position`: Job position

- **EmailRecipient**: Junction table for email recipients
  - `EmailId` (PK, FK): Reference to EmailFile
  - `ContactId` (PK, FK): Reference to Contact
  - `RecipientType`: Type (To, Cc, Bcc)

- **Topic**: Email topics for categorization
  - `TopicId` (PK): Unique identifier
  - `TopicName`: Topic name
  - `Keywords`: Related keywords

- **TopicDocumentMapping**: Junction table for email-topic relationships
  - `TopicId` (PK, FK): Reference to Topic
  - `FileId` (PK, FK): Reference to EmailFile
  - `RelevanceScore`: Topic relevance score

### Directory Structure

```
EnronEmailSearch/
├── Core/                # Domain models and business logic
│   ├── Interfaces/      # Core abstractions
│   ├── Models/          # Domain entities
│   ├── ScaleCube/       # Scaling implementations
│   └── Services/        # Core business services
├── Infrastructure/      # External concerns implementation
│   ├── Data/            # Database implementation
│   ├── DI/              # Dependency injection
│   └── Services/        # Infrastructure services
├── Indexer/             # Console application for indexing
│   ├── Program.cs       # Entry point for indexer
│   └── Dockerfile       # Container definition
├── Web/                 # Web interface
│   ├── Controllers/     # MVC controllers
│   ├── Views/           # Razor views
│   └── Dockerfile       # Container definition
└── Documentation/       # Project documentation
    └── C4Model.dsl      # C4 model definition
```

## Scale Cube Implementation

### X-Axis: Horizontal Duplication

The system implements X-axis scaling through multiple instances of the web application behind an NGINX load balancer. Configuration is managed through the `XAxisConfiguration` class and applied via docker-compose.

```yaml
services:
  webapp:
    # ...
    deploy:
      replicas: 2  # X-axis scaling - horizontal duplication
```

### Y-Axis: Functional Decomposition

The system can be deployed in a microservice configuration using `docker-compose.microservice.yml`, which separates functionality into distinct services:

- Cleaner Service
- Indexer Service
- Search API Service
- Web UI Service

### Z-Axis: Data Partitioning

Data processing is partitioned using various sharding strategies implemented in the `ZAxisScaling` class:

- Range-based sharding
- Hash-based sharding
- Directory-based sharding
- Modulo hash sharding

## Fault Isolation

The system implements comprehensive fault isolation using Polly patterns:

- **Circuit Breakers**: Prevent cascading failures
- **Retry Policies**: Handle transient errors
- **Timeout Policies**: Prevent hanging operations
- **Bulkheads**: Isolate components from each other

Key resilience classes:
- `ResilienceService`: Implements core resilience patterns
- `ResilientEmailIndexer`: Decorates the indexer with resilience

## Monitoring

Monitoring is implemented with:

- **Prometheus**: Metrics collection
- **Grafana**: Visualization dashboards
- **Health Checks**: Active monitoring of system components
- **Serilog**: Structured logging

## Getting Started

### Prerequisites

- Docker and Docker Compose
- The Enron email dataset (downloadable from https://www.cs.cmu.edu/~enron/)

### Installation and Running

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/EnronEmailSearch.git
   ```

2. Create directories for data:
   ```
   mkdir -p enron_dataset cleaned_emails db
   ```

3. Download and extract the Enron dataset to the `enron_dataset` directory.

4. Start the system:
   ```
   docker-compose up
   ```

5. For microservice deployment:
   ```
   docker-compose -f docker-compose.microservice.yml up
   ```

### Accessing the System

- Web Interface: http://localhost:80
- Grafana Dashboards: http://localhost:3000 (admin/admin)
- Prometheus: http://localhost:9090

## Performance

Performance metrics for the system:

| Configuration | Files/Second | Total Processing Time |
|---------------|-------------|------------------------|
| Single thread | 152         | 43 min                 |
| Multi-thread  | 487         | 14 min                 |
| Z-axis (4 shards) | 1,892   | 3.5 min                |
