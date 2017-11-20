/* This file is currently associated to an HTML file of the same name and is drawing content from it.  Until the files are disassociated, you will not be able to move, delete, rename, or make any other changes to this file. */

function DisplayTemplate_0f846447de5342f09b3e6d8e46f95da2(ctx) {
  var ms_outHtml=[];
  var cachePreviousTemplateData = ctx['DisplayTemplateData'];
  ctx['DisplayTemplateData'] = new Object();
  DisplayTemplate_0f846447de5342f09b3e6d8e46f95da2.DisplayTemplateData = ctx['DisplayTemplateData'];

  ctx['DisplayTemplateData']['TemplateUrl']='~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fItem_SAO_GridRow.js';
  ctx['DisplayTemplateData']['TemplateType']='Item';
  ctx['DisplayTemplateData']['TargetControlType']=['Content Web Parts'];
  this.DisplayTemplateData = ctx['DisplayTemplateData'];

  ctx['DisplayTemplateData']['ManagedPropertyMapping']={'Picture URL':['PublishingImage', 'PictureURL', 'PictureThumbnailURL'], 'Title':null, 'Path':null, 'SecondaryFileExtension':null, 'ContentTypeId':null, 'EditorOWSUSER':null, 'LastModifiedTime':null, 'DocId':null, 'ListItemID':null, 'HitHighlightedSummary':null, 'HitHighlightedProperties':null, 'FileExtension':null, 'ViewsLifeTime':null, 'ParentLink':null, 'FileType':null, 'IsContainer':null, 'DisplayAuthor':null, 'LinkFilename':null, 'Line 1':['Title'], 'Line 2':['Description'], 'Line 3':[], 'Line 4':[], 'Line 5':[], 'Line 6':[], 'Line 7':[], 'Line 8':[], 'Line 9':[], 'Line 10':[]};
  var cachePreviousItemValuesFunction = ctx['ItemValues'];
  ctx['ItemValues'] = function(slotOrPropName) {
    return Srch.ValueInfo.getCachedCtxItemValue(ctx, slotOrPropName)
};

ms_outHtml.push('',''
);
var encodedId = $htmlEncode(ctx.ClientControl.get_nextUniqueId() + "_Diagnostic_");

var linkURL = $getItemValue(ctx, "Path");
linkURL.overrideValueRenderer($urlHtmlEncode);

var line1 = $getItemValue(ctx, "Line 1");
var pictureURL = $getItemValue(ctx, "Picture URL");
pictureURL.overrideValueRenderer($urlHtmlEncode);
var pictureId = encodedId + "picture";
var pictureMarkup = Srch.ContentBySearch.getPictureMarkup(pictureURL, 100, 100, ctx.CurrentItem, "cbs-picture3LinesImg", line1, pictureId);

window.cbsDiagnostic_RenderPropertyMappings = function(valueInfoObj)
{
    var combinedManagedPropertiesMapping = "";
    if(!$isNull(valueInfoObj) && !$isNull(valueInfoObj.propertyMappings) && !$isNull(valueInfoObj.propertyMappings.length))
    {
        for (var i = 0; i < valueInfoObj.propertyMappings.length; i++)
        {
            var managedPropertyName = valueInfoObj.propertyMappings[i];
            combinedManagedPropertiesMapping += i == 0 ? managedPropertyName : String.format(Srch.Res.edisc_MultiValueFormat, managedPropertyName);
        }
    }
    return $htmlEncode(combinedManagedPropertiesMapping);
}

var itemContainerTitle = null;
var canBuildItemContainerTitle = !$isNull(ctx.CurrentItemIdx) && !$isNull(ctx.CurrentGroup) && 
    !$isNull(ctx.CurrentGroup.ResultRows) && !$isNull(ctx.CurrentGroup.ResultRows.length) &&
    !isNaN(ctx.CurrentGroup.ResultRows.length) && !isNaN(ctx.CurrentItemIdx);
if(canBuildItemContainerTitle)
{
    itemContainerTitle = String.format($resource("item_Diagnostic_ItemTitleFormat"), ctx.CurrentItemIdx + 1, ctx.CurrentGroup.ResultRows.length);
}

var containerId = encodedId + "container";
var pictureSlotContainerId = encodedId + "pictureSlotContainer";
var pictureContainerId = encodedId + "pictureContainer";
var pictureLinkId = encodedId + "pictureLink";
var pathContainerId = encodedId + "pathContainer";
ms_outHtml.push(''
,'                                     '
,'			'
,'		<tr class="gridRow">        '
);
		for(var lineNum = 1; lineNum <= 10; lineNum++)
		{
			var lineValueInfo = $getItemValue(ctx, String.format("Line {0}", lineNum));
			if(!$isNull(lineValueInfo) && !$isEmptyString(cbsDiagnostic_RenderPropertyMappings(lineValueInfo)))
			{
				var lineId = String.format("{0}line{1}", encodedId, lineNum);
				var slotName = String.format($resource("item_Diagnostic_SlotNameFormat"), lineNum);
		ms_outHtml.push(''
,'					'
);
				if(lineValueInfo.isEmpty)
				{
					if (ctx.CurrentItemIdx == 0)
						ctx.ManagedPropertyNames.push(lineValueInfo.managedPropertyName);

		ms_outHtml.push(''
,'				<td></td>'
);
				}
				else
				{
					if (ctx.CurrentItemIdx == 0)
						ctx.ManagedPropertyNames.push(lineValueInfo.managedPropertyName);

		ms_outHtml.push(''
,'					<td>', lineValueInfo ,'</td>'
);
				}
		ms_outHtml.push(''
,'							  '
);
			}
		}
		ms_outHtml.push(''
,'		</tr>'
,'				'
,'    '
);

  ctx['ItemValues'] = cachePreviousItemValuesFunction;
  ctx['DisplayTemplateData'] = cachePreviousTemplateData;
  return ms_outHtml.join('');
}
function RegisterTemplate_0f846447de5342f09b3e6d8e46f95da2() {

if ("undefined" != typeof (Srch) &&"undefined" != typeof (Srch.U) &&typeof(Srch.U.registerRenderTemplateByName) == "function") {
  Srch.U.registerRenderTemplateByName("Item_GridRow", DisplayTemplate_0f846447de5342f09b3e6d8e46f95da2);
}

if ("undefined" != typeof (Srch) &&"undefined" != typeof (Srch.U) &&typeof(Srch.U.registerRenderTemplateByName) == "function") {
  Srch.U.registerRenderTemplateByName("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fItem_SAO_GridRow.js", DisplayTemplate_0f846447de5342f09b3e6d8e46f95da2);
}
//
        $includeLanguageScript("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fItem_SAO_GridRow.js", "~sitecollection/_catalogs/masterpage/Display Templates/Language Files/{Locale}/CustomStrings.js");
    //
}
RegisterTemplate_0f846447de5342f09b3e6d8e46f95da2();
if (typeof(RegisterModuleInit) == "function" && typeof(Srch.U.replaceUrlTokens) == "function") {
  RegisterModuleInit(Srch.U.replaceUrlTokens("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fItem_SAO_GridRow.js"), RegisterTemplate_0f846447de5342f09b3e6d8e46f95da2);
}