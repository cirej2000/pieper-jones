/**
 * 
 */
package com.pieperjones.junit5.common.testrail.reporter;

import static java.lang.annotation.ElementType.METHOD;
import static java.lang.annotation.RetentionPolicy.RUNTIME;

import java.lang.annotation.Retention;
import java.lang.annotation.Target;

import com.pieperjones.junit5.common.extensions.TestrailReporterExtension;
import org.junit.jupiter.api.extension.ExtendWith;

/**
 * Use this as a decorator for individual JUNIT (or other tools) test cases.  Used to
 * put the testcase ID from testrail into the Junit automated test method.
 */
@Retention(RUNTIME)
@Target(METHOD)
@ExtendWith(TestrailReporterExtension.class)
public @interface TestrailCaseID {
	int value();
}
