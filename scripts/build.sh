#!/bin/bash
set -e

# Build steps:

dotnet build "$(dirname "$0")/CoursedogImporter.slnx"
