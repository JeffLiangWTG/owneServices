import React from "react";
import { createContext, useState } from "react";
import { TextFilterModule } from "./TextFilterModule";

interface FilterContextProps {
	filters: TextFilterModule[];
	setFilters: (filters: TextFilterModule[]) => void;
}

//generic
export const FilterContext = createContext<FilterContextProps>({
	filters: [],
	setFilters: (filters: TextFilterModule[]) => {},
});


//generic
export const FilterProvider = ({
    children,
  }: {
    children: React.ReactNode;
  }) => {
    const [filters, setFilters] = useState<TextFilterModule[]>([]);
    return (
      <FilterContext.Provider value={{ filters, setFilters }}>{children}</FilterContext.Provider>
    );
};
