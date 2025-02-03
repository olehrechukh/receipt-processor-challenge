
# Receipt Processor Challenge

## Summary
This project is a solution to the [Receipt Processor Challenge](https://github.com/fetch-rewards/receipt-processor-challenge/tree/main) provided by Fetch Rewards. 

The challenge involves creating an API that processes receipt data. The solution is implemented using C# and .NET 9, and the application is containerized with Docker for easy local rul.

## How to Run Locally (Using Local Build Image)

1. **Clone the repository**:
   ```bash
   git clone https://github.com/olehrechukh/receipt-processor-challenge
   cd receipt-processor-challenge
   ```

2. **Build the Docker Image Locally**:
      ```bash
      docker build -t olehrechukh-receipt-processor-challenge .
      ```

3. **Run the Application with Docker**:
      ```bash
      docker run -p 8080:8080 olehrechukh-receipt-processor-challenge
      ```
   The application will be accessible at `http://localhost:8080`.

4. **Test the API**:
    - Please check a scalar API client on: http://localhost:8080/scalar/
    - You can test the API using an [HTTP file](receipt.processor/receipt.processor.http) in VSCode [extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).
## How to Run from Docker Registry

1. **Pull the Docker Image from the Registry**:
     ```bash
     docker pull olehrechukh/receipt-processor-challenge:latest
     ```

2. **Run the Application from the Registry**:
      ```bash
      docker run -p 8080:8080 olehrechukh/receipt-processor-challenge:latest
      ```
   The application will be accessible at `http://localhost:8080`.

3. **Test the API**:
    - Please check a scalar API client on: http://localhost:8080/scalar/