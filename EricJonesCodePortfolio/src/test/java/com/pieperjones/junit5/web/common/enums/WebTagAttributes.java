package com.pieperjones.junit5.web.common.enums;

public enum WebTagAttributes {
	STYLE("style"),
	VALUE("value"),
	ALIGN("align"),
	WIDTH("width"),
	BORDER("border"),
	CELLPADDING("cellpadding"),
	CELLSPACING("cellspacing"),
	TYPE("type"),
	REL("rel"),
	DIV("div"),
	TABLE("table"),
	TR("tr"),
	TD("td"),
	OUTLINE("outline"),
	BODY("body");
	
	private String tagName;
	
	WebTagAttributes(String name){
		tagName = name;
	}
	
	public String getTagName() {
		return tagName;
	}

}
