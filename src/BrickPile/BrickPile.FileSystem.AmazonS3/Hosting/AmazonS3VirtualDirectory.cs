﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using BrickPile.Core.Hosting;

namespace BrickPile.FileSystem.AmazonS3.Hosting {
    /// <summary>
    /// 
    /// </summary>
    public class AmazonS3VirtualDirectory : CommonVirtualDirectory {
        private readonly AmazonS3VirtualPathProvider _provider;
        private readonly string _virtualPath;
        private readonly AmazonS3Client _client;
        /// <summary>
        /// Gets the AWS virtual path.
        /// </summary>
        protected string AWSVirtualPath {
            get {
                if(string.IsNullOrEmpty(_awsVirtualPath)) {
                    _awsVirtualPath = this._virtualPath.Replace(_provider.VirtualPathRoot, string.Empty);    
                }
                return _awsVirtualPath;
            }
        }
        private string _awsVirtualPath;
        /// <summary>
        /// Gets a list of all the subdirectories contained in this directory.
        /// </summary>
        /// <returns>An object implementing the <see cref="T:System.Collections.IEnumerable"/> interface containing <see cref="T:System.Web.Hosting.VirtualDirectory"/> objects.</returns>
        public override IEnumerable Directories {
            get {
                return this.GetFolder(this.VirtualPath.Replace(this._provider.VirtualPathRoot, string.Empty)).Select(amazonFolder => new AmazonS3VirtualDirectory(this._provider, this._provider.VirtualPathRoot + amazonFolder));
            }
        }
        /// <summary>
        /// Gets the folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        private IEnumerable<string> GetFolder(string folder) {
            var request = new ListObjectsRequest().WithBucketName(this._provider.BucketName).WithPrefix(folder);
            using (var response = this._client.ListObjects(request)) {

                if (folder == string.Empty || folder == "/") {
                    // get the objects at the TOP LEVEL, i.e. not inside any folders
                    var objects = response.S3Objects.Where(o => !o.Key.Contains(@"/"));

                    // get the folders at the TOP LEVEL only
                    return response.S3Objects.Except(objects).Where(o => o.Key.Last() == '/' && o.Key.IndexOf(@"/") == o.Key.LastIndexOf(@"/")).Select(n => n.Key);
                }
                
                var directories = new List<string>();

                foreach (var split in response.S3Objects.Select(s3Object => s3Object.Key.Replace(folder, string.Empty).Split('/')).Where(splits => splits.Count() > 1 && !directories.Contains(folder + splits.First()))) {
                    directories.Add(folder + split.First());
                }

                return directories;
            }
        }
        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        private IEnumerable<S3Object> GetFiles(string folder) {
            var request = new ListObjectsRequest().WithBucketName(this._provider.BucketName).WithPrefix(folder).WithDelimiter("/");
            using (var response = this._client.ListObjects(request)) {
                return response.S3Objects.Where(o => o.Key.Last() != '/');
            }
        }
        /// <summary>
        /// Gets a list of all files contained in this directory.
        /// </summary>
        /// <returns>An object implementing the <see cref="T:System.Collections.IEnumerable"/> interface containing <see cref="T:System.Web.Hosting.VirtualFile"/> objects.</returns>
        public override IEnumerable Files {
            get {
                return this.GetFiles(this.VirtualPath.Replace(this._provider.VirtualPathRoot, string.Empty)).Select(amazonFile => new AmazonS3VirtualFile(this._provider, this._provider.VirtualPathRoot + amazonFile.Key));
            }
        }
        /// <summary>
        /// Gets a list of the files and subdirectories contained in this virtual directory.
        /// </summary>
        /// <returns>An object implementing the <see cref="T:System.Collections.IEnumerable"/> interface containing <see cref="T:System.Web.Hosting.VirtualFile"/> and <see cref="T:System.Web.Hosting.VirtualDirectory"/> objects.</returns>
        public override IEnumerable Children {
            get {
                yield return Files;
                yield return Directories;
            }
        }
        /// <summary>
        /// Gets the parent.
        /// </summary>
        public override CommonVirtualDirectory Parent {
            get {
                var directory = VirtualPathUtility.GetDirectory(base.VirtualPath);
                return (_provider.GetDirectory(directory) as CommonVirtualDirectory);
            }
        }
        /// <summary>
        /// Unifieds the virtual file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override CommonVirtualFile CreateFile(string name) {
            return new AmazonS3VirtualFile(this._provider, VirtualPathUtility.Combine(this.VirtualPath, name));
        }
        /// <summary>
        /// Gets the display name of the virtual resource.
        /// </summary>
        /// <returns>The display name of the virtual file.</returns>
        public override string Name {
            get { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(base.Name); }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonS3VirtualDirectory"/> class.
        /// </summary>
        /// <param name="provider">The virtual path provider.</param>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        public AmazonS3VirtualDirectory(AmazonS3VirtualPathProvider provider, string virtualPath) : base(virtualPath) {
            _client = new AmazonS3Client(new AmazonS3Config {ServiceURL = "s3.amazonaws.com", CommunicationProtocol = Protocol.HTTP});
            _provider = provider;
            _virtualPath = virtualPath;
        }
    }
}