package com.pieperjones.junit5.common;

import java.net.MalformedURLException;

import com.pieperjones.junit5.common.extensions.StartupShutDownExtension;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.extension.ExtendWith;

@ExtendWith(StartupShutDownExtension.class)
/**
 * Base class for all testing types.  This allows us to enforce common behaviors amongst
 * all tests, regardless of whether they are DB, Integration, API, WEB, Mobile, et al.
 * The tie that binds.
 */
public abstract class BaseTest {

	final static Logger log = LogManager.getLogger(BaseTest.class);
	
	@BeforeAll
	protected abstract void setupTests() throws MalformedURLException;	
}
