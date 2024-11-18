# Campaign Management

##### Overview

    - Project in .Net Core 8.0, RESTful API and Blazor Web Assembly. Allows to create, read, update and delete, data from the database.
    This project is a campaign management. We can create users(Employees), customers, campaigns, and emails.
    This project implement the SOLID principles.

### Architecture Design Task

    - Design a system architecture diagram showcasing:
      • The overall architecture of the CDP highlights the .NET and Blazor components.
      • How data flows from ingestion to storage, analysis, and campaign management and analytics.
      • The integration points with external services (e.g., email delivery services).
      • Authentication and security mechanisms.

    To create the architecture diagram, you can use any diagramming tool you are comfortable with (e.g., draw.io, Lucidchart).

### Coding Challenge

    • API Development: Implement a RESTful API endpoint using .NET that accepts JSON payloads for customer data. The endpoint should validate and store the data in a database or memory.

    • Blazor Component: Develop a Blazor component responsible for sending customer data to the API

### Features

    - RESTful API;
    - CRUD methods;
    - MySQL;
    - Entity Framework Core;
    - Blazor Assembly;
    - .Env;
    - Json Web Token;
    - BCrypt;
    - UnitTests (xUnit);

### Project Description

    - This project start with a blank. Have a API(RESTful) endpoint project that accepts JSON payloads.
    - Have a Blazor Web Assembly project for UI, where users(Employees) interact, and this project do requests to the API.
    - Have a class library, for the domain models.
    - Have a class library, for the data transfer object's(DTO).
    - Have a class library, for the Enum.
    - Have a class library, for the services, to interact with the database.
    - Have a xUnit project, for the unit tests.

    - The application, have a login and register system, that can identify if username and email, is already use by another user(employee), as well, if the email for the customer is already taken.
    There is a hierarchy system(claims), that means, we can create a user, a manager and a admin. And this allows access or less, depending on the level, to the pages.

    - The Application, allows to create campaign(CRUD), customers(CRUD), Email(CRUD). Have pagination, relationships between models, validation for models.

    - The log in if we forget to log out, have a timer, and log out after this time.

### Installation

    - Clone the repository;
    - Create the database in your MySQL;
    - Configure the database connection string in the .Env file;
    - Create a .env file to put your credentials, and update the db config to use your info:
      CONNECTIONSTRING="your-connection-string"
      JWT_ISSUER_PRODUCTION="your-issuer-production"
      JWT_AUDIENCE_PRODUCTION="your-audience-production"
      JWT_SECRET="your-password"
      JWT_ISSUER_DEV="your-issuer-dev"
      JWT_AUDIENCE_DEV="your-audience-dev"
    - With your models, just create the appropriate migrations;

### Screenshots

    1 - Architecture Diagram;

<p align="center">
  <img src="./screenshots/architecture_diagram.png" width="350" title="Console">
</p>

    2 - Models Diagram;

<p align="center">
  <img src="./screenshots/models_diagram.png" width="350" title="Console">
</p>

    3 - Home Page;

<p align="center">
  <img src="./screenshots/home_page.png" width="350" title="Console">
</p>

    4 - Login Page;

<p align="center">
  <img src="./screenshots/login_page.png" width="350" title="Console">
</p>

    5 - Register Page;

<p align="center">
  <img src="./screenshots/register_page.png" width="350" title="Console">
</p>

    6 - List Emails;

<p align="center">
  <img src="./screenshots/list_emails.png" width="350" title="Console">
</p>

    7 - List Campaigns;

<p align="center">
  <img src="./screenshots/list_campaigns.png" width="350" title="Console">
</p>

    8 - List Customers;

<p align="center">
  <img src="./screenshots/list_customers.png" width="350" title="Console">
</p>

    9 - Add Email;

<p align="center">
  <img src="./screenshots/add_email.png" width="350" title="Console">
</p>

    10 - Add Campaign;

<p align="center">
  <img src="./screenshots/add_campaign.png" width="350" title="Console">
</p>

    11 - Add Customer;

<p align="center">
  <img src="./screenshots/add_customer.png" width="350" title="Console">
</p>

    12 - Edit Email;

<p align="center">
  <img src="./screenshots/edit_email.png" width="350" title="Console">
</p>

    13 - Edit Campaign;

<p align="center">
  <img src="./screenshots/edit_campaign.png" width="350" title="Console">
</p>

    14 - Edit Customer;

<p align="center">
  <img src="./screenshots/edit_customer.png" width="350" title="Console">
</p>

    15 - Delete Email;

<p align="center">
  <img src="./screenshots/delete_email.png" width="350" title="Console">
</p>

    16 - Delete Campaign;

<p align="center">
  <img src="./screenshots/delete_campaign.png" width="350" title="Console">
</p>

    17 - Delete Cutomer;

<p align="center">
  <img src="./screenshots/delete_cutomer.png" width="350" title="Console">
</p>

    18 - Search Emails;

<p align="center">
  <img src="./screenshots/search_emails.png" width="350" title="Console">
</p>

    19 - Search Emails with result;

<p align="center">
  <img src="./screenshots/search_emails1.png" width="350" title="Console">
</p>

    20 - About Page;

<p align="center">
  <img src="./screenshots/about_page.png" width="350" title="Console">
</p>
