using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using BrickPile.Core.Infrastructure.Indexes;
using BrickPile.Domain;
using BrickPile.Domain.Models;
using BrickPile.UI.Web.Mvc;
using BrickPile.UI.Web.Routing;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using StructureMap;

namespace BrickPile.Tests.Web.Routing {
    public class PathResolverTests {
        private IDocumentStore _store;
        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup() {
            _store = new EmbeddableDocumentStore { RunInMemory = true };
            _store.Initialize();
            _store.Conventions.FindTypeTagName = type => typeof(IPageModel).IsAssignableFrom(type) ? "Pages" : null;
            IndexCreation.CreateIndexes(typeof(PageByUrl).Assembly, _store);
        }
        /// <summary>
        /// Tears down.
        /// </summary>
        [TearDown]
        public void TearDown() {
            _store.Dispose();
        }
        /// <summary>
        /// Home_s the page_ with_ default_ action.
        /// </summary>
        /// <param name="path">The path.</param>
        [TestCase("")]
        [TestCase("/")]
        public void Home_Page_With_Default_Action(string path) {

            // Arrange
            var pathData = new PathData();
            var mapper = new Mock<IControllerMapper>();
            var container = new Mock<IContainer>();

            mapper.Setup(x => x.GetControllerName(typeof(DummyController))).Returns("Dummy");
            container.Setup(x => x.GetInstance<IDocumentSession>()).Returns(_store.OpenSession());

            // Act
            IPathData data;

            using (var session = _store.OpenSession()) {

                // create and store a new page model
                var pageModel = new DummyModel { Parent = null };
                session.Store(pageModel);
                session.SaveChanges();

                // try to resovle the page via the path
                var resolver = new PathResolver(session, pathData, mapper.Object, container.Object);
                data = resolver.ResolvePath(new RouteData(), path);

            }

            // Assert
            Assert.NotNull(data);
            Assert.AreEqual("index", data.Action);
            Assert.AreEqual("Dummy", data.Controller);
        }
        /// <summary>
        /// This test is based on a request for the page with the url ~/page with the default action index
        /// </summary>
        /// <param name="virtualUrl">The virtual URL.</param>
        [TestCase("/page")]
        [TestCase("/page/")]
        [TestCase("page")]
        public void Page_With_Default_Action(string virtualUrl) {

            // Arrange
            var pathData = new PathData();
            var mapper = new Mock<IControllerMapper>();
            var container = new Mock<IContainer>();

            mapper.Setup(x => x.GetControllerName(typeof(DummyController))).Returns("Dummy");
            container.Setup(x => x.GetInstance<IDocumentSession>()).Returns(_store.OpenSession());

            // Act
            IPathData data;
            using (var session = _store.OpenSession()) {
                // create and store a new page model
                var pageModel = new DummyModel { Metadata = { Url = "page" } };
                session.Store(pageModel);
                session.SaveChanges();

                var resolver = new PathResolver(session, pathData, mapper.Object, container.Object);
                data = resolver.ResolvePath(new RouteData(), virtualUrl);
            }

            // Assert
            Assert.NotNull(data);
            Assert.AreEqual("index", data.Action);
            Assert.AreEqual("Dummy", data.Controller);
        }

        /// <summary>
        /// This test is based on a request for the pate with url ~/page with the action myaction
        /// </summary>
        /// <param name="virtualUrl">The virtual URL.</param>
        [TestCase("/page/myaction")]
        [TestCase("/page/myaction/")]
        public void Page_With_Custom_Action(string virtualUrl) {

            // Arrange
            var pathData = new PathData();
            var mapper = new Mock<IControllerMapper>();
            var container = new Mock<IContainer>();

            mapper.Setup(x => x.GetControllerName(typeof(DummyController))).Returns("Dummy");
            container.Setup(x => x.GetInstance<IDocumentSession>()).Returns(_store.OpenSession());

            // Act
            IPathData data;
            using (var session = _store.OpenSession()) {
                // create and store a new page model
                var pageModel = new DummyModel { Metadata = { Url = "page" } };
                session.Store(pageModel);
                session.SaveChanges();

                var resolver = new PathResolver(session, pathData, mapper.Object, container.Object);
                data = resolver.ResolvePath(new RouteData(), virtualUrl);
            }

            // Assert
            Assert.NotNull(data);
            Assert.AreEqual("myaction", data.Action);
            Assert.AreEqual("Dummy", data.Controller);
        }

        /// <summary>
        /// Home_s the page_ with_ custom_ action.
        /// </summary>
        /// <param name="virtualUrl">The virtual URL.</param>
        [TestCase("/myaction")]
        [TestCase("/myaction/")]
        public void Home_Page_With_Custom_Action(string virtualUrl) {

            // Arrange
            var pathData = new PathData();
            var mapper = new Mock<IControllerMapper>();
            var container = new Mock<IContainer>();

            mapper.Setup(x => x.GetControllerName(typeof(DummyController))).Returns("Dummy");
            mapper.Setup(x => x.ControllerHasAction("Dummy", "myaction")).Returns(true);
            container.Setup(x => x.GetInstance<IDocumentSession>()).Returns(_store.OpenSession());

            mapper.Setup(m => m.ControllerHasAction("Content", "myaction")).Returns(true);

            // Act
            IPathData data;
            using (var session = _store.OpenSession()) {
                // create and store a new page model
                var pageModel = new DummyModel { Parent = null };
                session.Store(pageModel);
                session.SaveChanges();

                var resolver = new PathResolver(session, pathData, mapper.Object, container.Object);
                data = resolver.ResolvePath(new RouteData(), virtualUrl);
            }

            // Assert
            Assert.NotNull(data);
            Assert.AreEqual("myaction", data.Action);
            Assert.AreEqual("Dummy", data.Controller);
           
        }
        //[TestCase("/myaction/")]
        //public void Can_Lookup_Controller_Without_ControllerType_Specified(string virtualUrl) {
        //    // Arrange
        //    var pathData = new PathData();
        //    var mapper = new Mock<IControllerMapper>();
        //    var container = new Mock<IContainer>();

        //    mapper.Setup(x => x.GetControllerName(typeof(DummyModelWithoutControllerTypeController))).Returns("DummyModelWithoutControllerType");
        //    mapper.Setup(x => x.ControllerHasAction("DummyModelWithoutControllerType", "myaction")).Returns(true);
        //    container.Setup(x => x.GetInstance<IDocumentSession>()).Returns(_store.OpenSession());

        //    mapper.Setup(m => m.ControllerHasAction("DummyModelWithoutControllerType", "myaction")).Returns(true);

        //    // Act
        //    IPathData data;
        //    using (var session = _store.OpenSession()) {
        //        // create and store a new page model
        //        var pageModel = new DummyModelWithoutControllerType { Parent = null };
        //        session.Store(pageModel);
        //        session.SaveChanges();

        //        var resolver = new PathResolver(session, pathData, mapper.Object, container.Object);
        //        data = resolver.ResolvePath(new RouteData(), virtualUrl);
        //    }

        //    // Assert
        //    Assert.NotNull(data);
        //    Assert.AreEqual("myaction", data.Action);
        //    Assert.AreEqual("DummyModelWithoutControllerType", data.Controller);
            
        //}

        [TestCase("/mypage")]
        public void Can_Create_Model_WithOut_Inheritance(string url) {

            // Arrange
            var pathData = new PathData();
            var mapper = new Mock<IControllerMapper>();
            var container = new Mock<IContainer>();

            mapper.Setup(x => x.GetControllerName(typeof(DummyController))).Returns("Dummy");
            mapper.Setup(x => x.ControllerHasAction("Dummy", "mypage")).Returns(true);
            container.Setup(x => x.GetInstance<IDocumentSession>()).Returns(_store.OpenSession());

            // Act
            IPathData data;
            using (var session = _store.OpenSession()) {

                var siteMap = new SiteMap();
                session.Store(siteMap);
                // create and store a new page model
                var page = new DummyModel();
                //page.Childs.Add(new DummyModel {Metadata = { Name = "Child 1"}});

                var content = new StandardPage();
                session.Store(content);
                //page.PageReference.Id = content.Id;

                //var pageModel = new DummyModel();
                //session.Store(pageModel);
                //session.SaveChanges();

                var resolver = new PathResolver(session, pathData, mapper.Object, container.Object);
                data = resolver.ResolvePath(new RouteData(), url);
            }
            // Assert
            Assert.NotNull(data);
        }

        [Test]
        public void Can_Query_Page_Using_AllPages_Index() {

            // Arrange
            IPageModel data;

            // Act
            using (var session = _store.OpenSession()) {

                var pageModel = new DummyModel
                {
                    Id = "DummyPages/1",
                    Parent = null
                };
                session.Store(pageModel);
                session.SaveChanges();

            }
            using (var session = _store.OpenSession()) {

                //data = session.Query<IPageModel, AllPages>()
                //    .Customize(x => x.WaitForNonStaleResults())
                //    .SingleOrDefault(x => x.Parent == null);
            }

            // Assert
            //Assert.NotNull(data);
        }
    }

    [PageType(Name = "Standard page", ControllerType = typeof(DummyController))]
    public class StandardPage {

        public string Id { get; set; }

    }

    [PageType(Name = "Dummy", ControllerType = typeof(DummyController))]
    public class DummyModel : PageModel { }

    public class SiteMap {
        public DummyModel StartPage { get; set; }
    }

    [PageType]
    public class DummyModelWithoutControllerType : PageModel {}

    public class DummyController : Controller { }

    public class DummyModelWithoutControllerTypeController : Controller {}
}