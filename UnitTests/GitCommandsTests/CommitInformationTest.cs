﻿using System;
using System.Linq;
using GitCommands;
using NUnit.Framework;
using ResourceManager;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitCommandsTests
{
    [TestFixture]
    public class CommitInformationTest
    {

        [TestMethod]
        public void CanCreateCommitInformationFromFormatedData()
        {
            LinkFactory linkFactory = new LinkFactory();
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid().ToString();
            var parentGuid2 = Guid.NewGuid().ToString();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                          "John Doe (Acme Inc) <John.Doe@test.com>\n" +
                          authorUnixTime + "\n" +
                          "Jane Doe (Acme Inc) <Jane.Doe@test.com>\n" +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";

            var expectedHeader = "Author:      <a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date: 3 days ago (" + LocalizationHelpers.GetFullDateString(authorTime) + ")" + Environment.NewLine +
                                 "Committer:   <a href='mailto:Jane.Doe@test.com'>Jane Doe (Acme Inc) &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date: 2 days ago (" + LocalizationHelpers.GetFullDateString(commitTime) + ")" + Environment.NewLine +
                                 "Commit hash: " + commitGuid + Environment.NewLine +
                                 "Parent(s):   <a href='gitext://gotocommit/" + parentGuid1 + "'>" + parentGuid1.Substring(0, 10) + "</a> <a href='gitext://gotocommit/" + parentGuid2 + "'>" + parentGuid2.Substring(0, 10) + "</a>";

            var expectedBody = "\nI made a really neato change." + Environment.NewLine + Environment.NewLine +
                               "Notes (p4notes):" + Environment.NewLine +
                               "\tP4@547123";

            // TEMP, will be refactored in the follow up refactor
            var commitData = new CommitDataManager(() => new GitModule("")).CreateFromFormatedData(rawData);
            var commitInformation = CommitInformation.GetCommitInfo(commitData, linkFactory, true);

            Assert.AreEqual(expectedHeader, commitInformation.Header);
            Assert.AreEqual(expectedBody, commitInformation.Body);
        }

        [TestMethod]
        public void CanCreateCommitInformationFromFormatedDataThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => CommitInformation.GetCommitInfo(null, new LinkFactory(), true));
        }

        [TestMethod]
        public void GetCommitInfoTestWhenDataIsNull()
        {
            var actualResult = CommitInformation.GetCommitInfo(new GitModule(""), new LinkFactory(), "fakesha1");
            Assert.AreEqual("Cannot find commit fakesha1", actualResult.Header);
        }

        [TestMethod]
        public void GetAllBranchesWhichContainGivenCommitTestReturnsEmptyList()
        {
            var module = new GitModule("");
            var actualResult = module.GetAllBranchesWhichContainGivenCommit("fakesha1", false, false);

            Assert.IsNotNull(actualResult);
            Assert.IsTrue(!actualResult.Any());
        }
    }
}
