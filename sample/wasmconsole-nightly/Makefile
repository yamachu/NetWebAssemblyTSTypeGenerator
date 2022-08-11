DOTNET:=$(shell which dotnet)

build:
	echo $(shell pwd)/types
	$(DOTNET) clean
	$(DOTNET) build /p:TypeOutputDir=$(shell pwd)/types
