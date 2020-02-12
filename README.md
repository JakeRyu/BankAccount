## Money transfer & withdrawal

This is a practice to simulates bank account transfer and withdrawal. The solution contains a .NET core library which is structured into the following 3 folders:
                                                                      
* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, `transfer money` and `withdraw money`
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

Some points to make are,
* Make our domain models rich in behaviour and much more than plain old objects
* Avoid data persistence operation (i.e. data access repository) to bleed into our domain

The code is self-explanatory with a good test coverage.
