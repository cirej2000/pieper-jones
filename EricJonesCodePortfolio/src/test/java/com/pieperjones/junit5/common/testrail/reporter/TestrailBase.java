/**
 * 
 */
package com.pieperjones.junit5.common.testrail.reporter;

import static java.lang.annotation.RetentionPolicy.RUNTIME;

import java.lang.annotation.Retention;

import com.pieperjones.junit5.common.extensions.BaseTestExtension;
import org.junit.jupiter.api.extension.ExtendWith;


/**
//* Description:    This annotation is used to decorate base test classes for different 
//* platforms (i.e., WEB, API, MOBILE, etc.)...identifies the class as enabled for testrail
//* integration.
/********************************************************************************************/
@Retention(RUNTIME)
//@Target(PACKAGE)
@ExtendWith(BaseTestExtension.class)
public @interface TestrailBase {

}
