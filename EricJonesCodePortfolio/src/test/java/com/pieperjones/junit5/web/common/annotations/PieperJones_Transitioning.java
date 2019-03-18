package com.pieperjones.junit5.web.common.annotations;

import com.pieperjones.junit5.web.common.enums.WebTagAttributes;

import static java.lang.annotation.ElementType.FIELD;
import static java.lang.annotation.ElementType.PARAMETER;
import static java.lang.annotation.RetentionPolicy.RUNTIME;

import java.lang.annotation.Retention;
import java.lang.annotation.Target;

@Retention(RUNTIME)
@Target({ FIELD, PARAMETER })
public @interface PieperJones_Transitioning {
	public WebTagAttributes attributeName();
	public String valueTextBefore();
	public String valueTextAfter();
}

