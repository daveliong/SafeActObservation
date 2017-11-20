//REFERENCE: http://www.eliostruyf.com/create-a-load-more-results-link-button-for-the-content-search-web-part/

//Function to show more result link
function ShowMoreResults (ctx, controlId) {

	//Create new element outside the web part body
	var showPaging = ctx.ClientControl.get_showPaging();
	
	if(showPaging)
	{
	
		var pagingInfo = ctx.ClientControl.get_pagingInfo();
		showPaging = !$isEmptyArray(pagingInfo);
		
		if(showPaging)
		{
		
			var hiddenElm = $('#'+controlId);
			var visibleElm = hiddenElm.parents('.ms-WPBody:eq(0)').parent().children('#Show-Items');
			// Hide the original set of results
			hiddenElm.hide();

			// Check if the Visible new element items element already exists. If not create it
			if (visibleElm.length <= 0) {
				// Get the tag name of the element
				var tagname = hiddenElm.prop('tagName');
				// Box needs to be created before or after the web part, otherwise the content will be cleared when new results are retrieved.
				hiddenElm.parents('.ms-WPBody:eq(0)').before('<'+tagname+' id="Show-Items" class="'+hiddenElm.attr('class')+'"></'+tagname+'>');
				visibleElm = hiddenElm.parents('.ms-WPBody:eq(0)').parent().children('#Show-Items');
			}

			// Append all the items from your result set to the new element
			hiddenElm.children().each(function () {
				//$('#Show-Items').append($(this).clone(true));

				// Append the result items to the visible div
				$(this).appendTo(visibleElm);
			});

			// Get the paging information
			var pagingInfo = ctx.ClientControl.get_pagingInfo();
			var firstPage = pagingInfo[0];
			var lastPage = pagingInfo[pagingInfo.length -1];
			// If the value of pageNumber is equal to -2, more results can be retrieved
			var hasNextPage = lastPage.pageNumber == -2;
			var hasPreviousPage = firstPage.pageNumber == -1;

			// Append the show more link if a next page is available
			if(hasPreviousPage)
			{
				hiddenElm.after('<a href="#" id="'+controlId+'showprevious" class="previousSearchResults">Show Previous Results</a>');
			}
					
			if(hasNextPage)
			{
				hiddenElm.after('<a href="#" id="'+controlId+'shownext" class="nextSearchResults">Show Next Results</a>');
			}


			// When clicked on the show more link, the new set of results needs to be retrieved
			$('#'+controlId+'showprevious').click(function () {
				// Load the previous set of results
				ctx.ClientControl.page(firstPage.startItem);
				return false;
			});

			$('#'+controlId+'shownext').click(function () {
				// Load the next set of results
				ctx.ClientControl.page(lastPage.startItem);
				return false;
			});
		}
	}
	
}