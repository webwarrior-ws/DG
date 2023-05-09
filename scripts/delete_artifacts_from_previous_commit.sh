#!/bin/bash

if [ -z $GITHUB_REPOSITORY ]; then
    echo "This script is meant to be used only within a GitHubCI pipeline."
    exit 1
fi

if [ -z $GITHUB_TOKEN ]; then
    echo "Please set the GITHUB_TOKEN environment variable in your GitHubCI pipeline:
    env:
        GITHUB_TOKEN: \${{ secrets.GITHUB_TOKEN }}
    "
    exit 1
fi

previous_commit_hash=$(git log --pretty=format:%H | head --lines 2 | tail --lines 1)

artifact_ids=$(curl \
    --header "Authorization: Bearer $GITHUB_TOKEN" \
    "https://api.github.com/repos/$GITHUB_REPOSITORY/actions/artifacts" \
    | jq '.artifacts[] | select( .workflow_run.head_sha == "'$previous_commit_hash'") | select( .name == "publishedPackages") | .id')

for artifact_id in $artifact_ids; do
    curl \
        --location \
        --request DELETE \
        --header "Accept: application/vnd.github+json" \
        --header "Authorization: Bearer $GITHUB_TOKEN" \
        --header "X-GitHub-Api-Version: 2022-11-28" \
        https://api.github.com/repos/$GITHUB_REPOSITORY/actions/artifacts/$artifact_id
done

