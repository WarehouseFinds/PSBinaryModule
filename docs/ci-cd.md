# CI/CD Pipeline Documentation

This document explains the continuous integration and deployment pipeline for PSBinaryModule.

## Overview

The CI/CD pipeline is implemented using GitHub Actions with two main workflows:

1. **CI Workflow** (`ci.yml`) - Runs on every push and pull request
2. **Release Workflow** (`release.yml`) - Runs on pushes to main branch and creates releases

## CI Workflow

### Triggers

- Push to `main` branch
- Pull requests to `main`, `release/*`, or `hotfix/*` branches
- Manual workflow dispatch
- Changes to relevant files (src, tests, build files)

### Jobs

#### 1. Changes (Label Changes)
- **Runs on**: ubuntu-latest
- **Condition**: Pull requests only
- **Purpose**: Automatically labels PRs based on changed files
- **Permissions**: contents:read, pull-requests:write

#### 2. Unit Tests
- **Runs on**: ubuntu-latest, windows-latest, macos-latest (matrix)
- **Purpose**: Run C# unit tests on all platforms
- **Steps**:
  1. Checkout repository
  2. Setup .NET 8.0
  3. Restore NuGet packages
  4. Build solution (Release configuration)
  5. Run dotnet test with code coverage
  6. Upload test results as artifacts
  7. Publish test results (Ubuntu only)

**Artifacts Produced:**
- `test-results-{os}` - Test result files (.trx) and code coverage

#### 3. Build
- **Runs on**: ubuntu-latest
- **Depends on**: unit-tests
- **Purpose**: Build the PowerShell module
- **Steps**:
  1. Checkout with full history (for GitVersion)
  2. Install and run GitVersion
  3. Setup .NET 8.0
  4. Install InvokeBuild
  5. Build module with semantic version
  6. Upload module artifact

**Artifacts Produced:**
- `PSBinaryModule` - Built module ready for distribution

**Outputs:**
- `release-version` - Semantic version from GitVersion

#### 4. Integration Tests
- **Runs on**: ubuntu-latest, windows-latest, macos-latest (matrix)
- **Depends on**: build
- **Purpose**: Test the built module in real PowerShell environments
- **Steps**:
  1. Checkout repository
  2. Download built module artifact
  3. Install Pester
  4. Run integration tests
  5. Upload test results

**Artifacts Produced:**
- `integration-test-results-{os}` - Integration test results

## Release Workflow

### Triggers

- Push to `main` branch (with changes to src/tests)
- Manual workflow dispatch with optional force-publish flag

### Jobs

#### Release
- **Runs on**: ubuntu-latest
- **Purpose**: Create GitHub release and publish to PowerShell Gallery
- **Steps**:
  1. Checkout with full history
  2. Determine version with GitVersion
  3. Setup .NET 8.0
  4. Install InvokeBuild and Pester
  5. Build and test module
  6. Create GitHub release with artifacts
  7. Publish to PowerShell Gallery (if not prerelease)

**GitHub Release:**
- Tag: `v{version}` (e.g., v1.2.3)
- Name: `Release v{version}`
- Artifacts: All module files
- Prerelease flag if version has prerelease label

**PowerShell Gallery:**
- Only publishes non-prerelease versions
- Requires `PSGALLERY_API_KEY` secret

## GitVersion Configuration

Versioning follows GitVersion's `ContinuousDeployment` mode:

### Branch Strategies

| Branch Pattern | Tag | Increment | Example |
|---------------|-----|-----------|---------|
| `main` | (none) | Minor | 1.2.0 |
| `release/*` | beta | Patch | 1.2.1-beta.1 |
| `feature/*` | alpha | Minor | 1.3.0-alpha.feature-name.1 |
| `hotfix/*` | hotfix | Patch | 1.2.1-hotfix.1 |
| `pr/*` | pr | Inherit | 1.2.0-pr.branch-name.1 |

### Version Flow Example

```
Initial commit → 0.1.0

feature/add-cmdlet → 0.2.0-alpha.add-cmdlet.1
  ↓ (merge to main)
main → 0.2.0

feature/another → 0.3.0-alpha.another.1
  ↓ (merge to main)
main → 0.3.0

release/1.0 → 1.0.0-beta.1
  ↓ (fixes)
release/1.0 → 1.0.0-beta.2
  ↓ (merge to main)
main → 1.0.0

hotfix/critical-bug → 1.0.1-hotfix.1
  ↓ (merge to main)
main → 1.0.1
```

## Secrets Configuration

### Required Secrets

Add these secrets in GitHub repository settings:

#### PSGALLERY_API_KEY
- **Purpose**: Publish module to PowerShell Gallery
- **How to get**:
  1. Log in to [PowerShell Gallery](https://www.powershellgallery.com/)
  2. Go to Account Settings → API Keys
  3. Create new API key with publish permissions
  4. Copy the key (you won't see it again!)
  5. Add as repository secret in GitHub

#### Optional Secrets

- **CODECOV_TOKEN**: For code coverage reporting to Codecov
- **SONAR_TOKEN**: For SonarCloud code quality analysis

## Environment Variables

The workflows use these environment variables:

- `DOTNET_CLI_TELEMETRY_OPTOUT`: Disable .NET telemetry
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE`: Speed up builds
- `NUGET_API_KEY`: PSGallery API key (from secrets)

## Workflow Customization

### Changing Test Matrix

To test on different OS versions, modify the matrix in `ci.yml`:

```yaml
strategy:
  matrix:
    os: [ubuntu-20.04, windows-2022, macos-13]
```

### Adding Code Quality Tools

#### SonarCloud

Add to unit-tests job:

```yaml
- name: SonarCloud Scan
  uses: SonarSource/sonarcloud-github-action@master
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

#### Codecov

Add after test step:

```yaml
- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v4
  with:
    files: ./test-results/codecoverage/code-coverage.xml
```

### Custom Build Steps

To add custom build steps, modify the Build job in `ci.yml`:

```yaml
- name: Custom Build Step
  shell: pwsh
  run: |
    # Your custom PowerShell code
    Write-Host "Running custom build step"
```

## Troubleshooting

### CI Failures

#### "GitVersion not found"
- Ensure GitVersion.yml is in repository root
- Check GitVersion action version compatibility

#### "Module not found"
- Check build artifacts were created
- Verify artifact upload/download paths match

#### "Test failures"
- Review test output in GitHub Actions logs
- Download test result artifacts for detailed analysis
- Run tests locally with same .NET version

### Release Issues

#### "PSGallery publish failed"
- Verify API key is valid and has publish permissions
- Check module manifest GUID is correct
- Ensure version doesn't already exist on PSGallery
- Check module name is available

#### "GitHub release failed"
- Ensure GITHUB_TOKEN has sufficient permissions
- Check tag doesn't already exist
- Verify branch protection rules allow pushes

## Manual Workflows

### Manual Release

Trigger a release manually:

1. Go to Actions → Release workflow
2. Click "Run workflow"
3. Select branch (usually main)
4. Check "force-publish" if needed
5. Click "Run workflow"

### Manual CI Run

Trigger CI manually:

1. Go to Actions → CI workflow
2. Click "Run workflow"
3. Select branch
4. Click "Run workflow"

## Best Practices

### Commit Messages

Use conventional commits for better changelog generation:

```
feat: add new cmdlet
fix: resolve pipeline issue
docs: update README
test: add integration tests
chore: update dependencies
```

### Pull Requests

1. Create feature branch: `feature/my-feature`
2. Make changes and commit
3. Push and create PR to main
4. Wait for CI to pass
5. Request review
6. Merge after approval

### Versioning

- **Breaking changes**: Merge to main, manually create release/2.0 branch
- **New features**: Regular merge to main (auto-increment minor)
- **Bug fixes**: Create hotfix branch, merge to main
- **Prerelease**: Use release/* or feature/* branches

## Monitoring

### GitHub Actions

Monitor workflow runs:
- Repository → Actions tab
- View logs for each job
- Download artifacts if needed
- Re-run failed jobs

### Build Status

Add badges to README:

```markdown
[![CI](https://github.com/user/repo/actions/workflows/ci.yml/badge.svg)](https://github.com/user/repo/actions/workflows/ci.yml)
```

### PowerShell Gallery

Monitor module statistics:
- [PowerShell Gallery](https://www.powershellgallery.com/)
- View download counts
- Check version history
- Monitor user feedback

## Advanced Topics

### Conditional Publishing

Publish only for certain versions:

```yaml
- name: Publish to PSGallery
  if: |
    github.event_name == 'push' && 
    !contains(steps.gitversion.outputs.semVer, '-')
```

### Multi-Stage Deployment

Deploy to test gallery first:

```yaml
- name: Publish to Test Gallery
  run: Publish-Module -Repository PSGalleryTest -NuGetApiKey ${{ secrets.TEST_API_KEY }}

- name: Verify in Test
  run: |
    Install-Module PSBinaryModule -Repository PSGalleryTest
    Test-Module

- name: Publish to Production
  if: success()
  run: Publish-Module -NuGetApiKey ${{ secrets.PROD_API_KEY }}
```

### Parallel Testing

Run tests in parallel:

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest, macos-latest]
    test-suite: [unit, integration]
  fail-fast: false
```

## Support

For issues with CI/CD:
1. Check GitHub Actions logs
2. Review this documentation
3. Open an issue with workflow run URL
4. Check GitHub Actions status page
