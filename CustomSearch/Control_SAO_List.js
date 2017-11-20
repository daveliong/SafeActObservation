/* This file is currently associated to an HTML file of the same name and is drawing content from it.  Until the files are disassociated, you will not be able to move, delete, rename, or make any other changes to this file. */

function DisplayTemplate_c89cc10b6bb2412a9656d5d5d8005d79(ctx) {
  var ms_outHtml=[];
  var cachePreviousTemplateData = ctx['DisplayTemplateData'];
  ctx['DisplayTemplateData'] = new Object();
  DisplayTemplate_c89cc10b6bb2412a9656d5d5d8005d79.DisplayTemplateData = ctx['DisplayTemplateData'];

  ctx['DisplayTemplateData']['TemplateUrl']='~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js';
  ctx['DisplayTemplateData']['TemplateType']='Control';
  ctx['DisplayTemplateData']['TargetControlType']=['Content Web Parts'];
  this.DisplayTemplateData = ctx['DisplayTemplateData'];

ms_outHtml.push('',''
,''
); 
if (!$isNull(ctx.ClientControl) &&
    !$isNull(ctx.ClientControl.shouldRenderControl) &&
    !ctx.ClientControl.shouldRenderControl())
{
    return "";
}

ctx.ListDataJSONGroupsKey = "ResultTables";
var $noResults = Srch.ContentBySearch.getControlTemplateEncodedNoResultsMessage(ctx.ClientControl);
var noResultsClassName = "ms-srch-result-noResults";
var encodedId = $htmlEncode(ctx.ClientControl.get_nextUniqueId() + "_Table_");
var headerRowId = $htmlEncode(encodedId + "_HeaderRow_");
ctx.ManagedPropertyNames = [];

ctx.OnPostRender = [];
ctx.OnPostRender.push(function () {
    for(var i = 0; i < ctx.ManagedPropertyNames.length; i++)
    {
        $(".resultsTableHeader").append("<th>" + ctx.ManagedPropertyNames[i] + "</th>");            
    }
});


var ListRenderRenderWrapper = function(itemRenderResult, inCtx, tpl)
{
    var iStr = [];
    iStr.push(itemRenderResult);
    return iStr.join('');
}
ctx['ItemRenderWrapper'] = ListRenderRenderWrapper;
ms_outHtml.push(''
,'	<table id="', encodedId ,'" class="resultsTable">'
,'		<thead>'
,'			<tr id="', headerRowId ,'" class="resultsTableHeader"></tr>'
,'		</thead>'
,'		<tbody>'
,'            ', ctx.RenderGroups(ctx) ,''
,'		 </tbody>'
,'    </table>    '
);
if (ctx.ClientControl.get_shouldShowNoResultMessage())
{
ms_outHtml.push(''
,'        <div class="', noResultsClassName ,'">', $noResults ,'</div>'
);
}
ms_outHtml.push(''
,''
,'    '
);

  ctx['DisplayTemplateData'] = cachePreviousTemplateData;
  return ms_outHtml.join('');
}
function RegisterTemplate_c89cc10b6bb2412a9656d5d5d8005d79() {

if ("undefined" != typeof (Srch) &&"undefined" != typeof (Srch.U) &&typeof(Srch.U.registerRenderTemplateByName) == "function") {
  Srch.U.registerRenderTemplateByName("Control_Grid", DisplayTemplate_c89cc10b6bb2412a9656d5d5d8005d79);
}

if ("undefined" != typeof (Srch) &&"undefined" != typeof (Srch.U) &&typeof(Srch.U.registerRenderTemplateByName) == "function") {
  Srch.U.registerRenderTemplateByName("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js", DisplayTemplate_c89cc10b6bb2412a9656d5d5d8005d79);
}
//
        $includeLanguageScript("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js", "~sitecollection/_catalogs/masterpage/Display Templates/Language Files/{Locale}/CustomStrings.js");
		$includeScript("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js", "https://ajax.aspnetcdn.com/ajax/jquery/jquery-2.1.0.min.js");
		$includeScript("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js", "https://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.9.4/jquery.dataTables.min.js");
		$includeCSS("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js", "https://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.9.4/css/jquery.dataTables.css");

    //
}
RegisterTemplate_c89cc10b6bb2412a9656d5d5d8005d79();
if (typeof(RegisterModuleInit) == "function" && typeof(Srch.U.replaceUrlTokens) == "function") {
  RegisterModuleInit(Srch.U.replaceUrlTokens("~sitecollection\u002f_catalogs\u002fmasterpage\u002fDisplay Templates\u002fContent Web Parts\u002fControl_SAO_List.js"), RegisterTemplate_c89cc10b6bb2412a9656d5d5d8005d79);
}