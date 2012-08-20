/* Copyright (C) 2011 by Marcus Lindblom

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

using System.Linq;
using BrickPile.Domain.Models;

namespace BrickPile.UI {
    /// <summary>
    /// Represents the navigation structure of the site
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class StructureInfo : IStructureInfo {
        /// <summary>
        /// Represents the current page with it's children, all ancestors including each ancestors children, primarily this is used for rendering the navigation.
        /// </summary>
        /// <value>
        /// The pages.
        /// </value>
        public IQueryable<IPageModel> NavigationContext { get; set; }
        /// <summary>
        /// Gets or sets the root page of the web site.
        /// </summary>
        /// <value>
        /// The root page.
        /// </value>
        public IPageModel StartPage { get; set; }
        /// <summary>
        /// Gets or sets the parent page.
        /// </summary>
        /// <value>
        /// The parent page.
        /// </value>
        public IPageModel ParentPage { get; set; }
    }        
}
