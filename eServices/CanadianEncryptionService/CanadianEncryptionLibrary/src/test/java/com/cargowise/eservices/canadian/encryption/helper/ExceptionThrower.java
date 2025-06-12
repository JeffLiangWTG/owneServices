package com.cargowise.eservices.canadian.encryption.helper;

@FunctionalInterface
public interface ExceptionThrower {
	void throwException() throws Exception;
}
