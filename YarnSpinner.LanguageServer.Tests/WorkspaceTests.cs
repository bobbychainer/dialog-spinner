using Xunit;
using System.Linq;
using FluentAssertions;
using System.IO;
using YarnLanguageServer.Diagnostics;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace YarnLanguageServer.Tests
{
    public class WorkspaceTests
    {
        private static string Project1Path = Path.Combine(TestUtility.PathToTestWorkspace, "Project1", "Project1.yarnproject");
        private static string Project2Path = Path.Combine(TestUtility.PathToTestWorkspace, "Project2", "Project1.yarnproject");
        private static string NoProjectPath = Path.Combine(TestUtility.PathToTestWorkspace, "FilesWithNoProject");

        [Fact]
        public void Projects_CanOpen()
        {
            // Given
            var project = new Project(Project1Path);
        
            // When
            project.ReloadProjectFromDisk(false);

            // Then
            project.Files.Should().HaveCount(1);
            project.Nodes.Should().HaveCount(2);
            project.Files.Should().AllSatisfy(file => file.Project.Should().Be(project));

            var testFilePath = DocumentUri.FromFileSystemPath(Path.Combine(TestUtility.PathToTestWorkspace, "Project1", "Test.yarn"));

            project.MatchesUri(Project1Path).Should().BeTrue();
            project.MatchesUri(testFilePath).Should().BeTrue();
        }

        [Fact]
        public void Workspaces_CanOpen() {
            var workspace = new Workspace();
            workspace.Root = TestUtility.PathToTestWorkspace;
            workspace.Initialize(null);

            var diagnostics = workspace.GetDiagnostics();

            workspace.Projects.SelectMany(p => p.Nodes).Should().NotBeEmpty();

            workspace.Projects.Should().HaveCount(2);

            // The node NotIncludedInProject is inside a file that is not
            // included in a .yarnproject; because we have opened a workspace
            // that includes .yarnprojects, the file will not be included
            workspace.Projects.Should().AllSatisfy(p => p.Nodes.Should().NotContain(n => n.Title == "NotIncludedInProject"));

            var firstProject = workspace.Projects.Should().ContainSingle(p => p.Uri.Path.Contains("Project1.yarnproject")).Subject;
            var fileInFirstProject = firstProject.Files.Should().ContainSingle().Subject;

            // Validate that diagnostics are being generated by looking for a warning that 
            // '<<unknown_command>>' is being warned about.
            var fileDiagnostics = diagnostics.Should().ContainKey(fileInFirstProject.Uri).WhoseValue;
            fileDiagnostics.Should().NotBeEmpty();
            fileDiagnostics.Should().Contain(d => d.Code!.Value.String == nameof(YarnDiagnosticCode.YRNMsngCmdDef) && d.Message.Contains("unknown_command"));
        }

        [Fact]
        public void Workspaces_WithNoProjects_HaveImplicitProject()
        {
            // Given
            var workspace = new Workspace();
            workspace.Root = NoProjectPath;
            workspace.Initialize(null);

            // Then
            var project = workspace.Projects.Should().ContainSingle().Subject;
            var file = project.Files.Should().ContainSingle().Subject;
            file.NodeInfos.Should().Contain(n => n.Title == "NotIncludedInProject");
        }

        [Fact]
        public void Workspaces_WithDefinitionsFile_UseDefinitions()
        {
            // Given
            var workspace = new Workspace();
            workspace.Root = Project2Path;
            workspace.Initialize(null);

            // When

            // Then
        }
    }
}
