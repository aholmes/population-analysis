# Report and Presentation

The report and presentation on population data can be reviewed as [HTML](report/presentation.html) or a Markdown [MARP](report/presentation.md) file under the [report/](report/) directory. The data used for this report can be found in the files [population.csv](report/population.csv) (formatted as per the prompt) and [population.raw.csv](report/population.raw.csv) (raw data used for analysis).

The code used to generate the charts found in the report are in a [Jupyter Notebook](report/PopulationAnalysis.ipynb).

# Run the Application

The application is written in C#; the software is in [Analysis/](Analysis/).

The application can run two ways:

- In a Docker container
- Natively through the .NET runtime

## Docker

1. Create a directory for the CSV outputs
2. Run the `docker` command, mounting the directory you created to `/csv_destination`:

```sh
docker run \
    -u $(id -u):$(id -g) \
    -v /the/output/directory:/csv_destination \
    --rm -it \
    aholmes0/population-analysis:latest
```

The Docker image can be found on [dockerhub](https://hub.docker.com/repository/docker/aholmes0/population-analysis/general).

**Note** the Docker image is a multi-arch build and _should_ support both AMD64 and ARM64 MacBooks. I do not have a MacBook with which to test the AMD64 image; please reach out with any errors so I may resolve them.

### Output

The CSV data is output to the mounted Docker container volume. The file `population.csv` is the formatted CSV as per the prompt, while `population.raw.csv` contains raw CSV data used in the Jupyter Notebook analysis.

## .NET

1. Install the .NET SDK https://dotnet.microsoft.com/en-us/download
2. Run the project:

```sh
dotnet restore && \
dotnet run --project Analysis/
```

### Output

The CSV data is output to the filename entered at the command line prompt when the application runs. The output file contains the formatted CSV as per the prompt. A file with the extension  `.raw.csv` is output as well; this files content is appropriate for further analysis.

### Automated Tests

Automated unit tests can be executed with `dotnet test`.
