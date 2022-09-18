DOTNET:=$(shell which dotnet)

build:
	$(DOTNET) clean
	$(DOTNET) build /p:TypeOutputDir=$(shell pwd)/generated
