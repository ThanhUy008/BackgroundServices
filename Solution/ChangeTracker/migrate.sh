#!/bin/bash
set -e

dotnet ef database update --context AppDbContext --project Solution/Shared/Shared.csproj
dotnet ef database update --context CustomerDbContext --project Solution/Shared/Shared.csproj