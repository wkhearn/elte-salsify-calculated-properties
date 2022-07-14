LET LeadWeeks = ROUND(DIVIDE(REGEX_MATCHES(VALUE("LeadDays"),"^\\d*"),7),0) IN

LET POdate = IF (VALUE("Date of Next Available PO"),                                #Check if NextAvailablePODate
                 
                IF(GT(VALUE("Date of Next Available PO"),TODAY()),                  #Check if date is in the future
                    FORMAT_DATETIME(VALUE("Date of Next Available PO"),'%Y-%m-%d', '%B %d'), #If yes, then display the future MM -YYYY
                    FORMAT_DATETIME(ADD(TODAY(),14),'%Y-%m-%d', '%B %d')) #If NO, then display the (current date + 14) in MM -YYYY
                ,
                
                 IF ( EQUAL(LeadWeeks,1),
                     CONCATENATE("in ",LeadWeeks," week"),
                     CONCATENATE("in ",LeadWeeks," weeks")
                
                    )
                
                ) IN




LET qty =
REGEX_MATCHES(
      VALUE("NetAvailByLoc"),
      "\\d*")
IN


LET totalinv =
ROUND(ARRAY_SUM(qty),0)
IN


LET sale = IF(LT(VALUE("NormalPrice"),VALUE("ProductPrice")), VALUE("NormalPrice"),VALUE("ProductPrice") ) IN 

LET IsonSale = IF(LT(sale,VALUE("MSRP")),"Yes","No") IN



IF (VALUE("ObsoleteStatusCode"),       #Check if obsolete value exists to exclude messages on Parents in Salsify

IF 																												
( 
	  GT(totalinv,"0") 		#Level0 IF and Condition - Is in Stock?	
	  
	   , 	                                 #Level0 True - IN STOCK				
	  
	         IF		 		#Level1 IF
	         (
		          AND			#Level1 Condition(Is D/T/OTB and Is on Sale is Yes and [clearance or Floor Model]?)
		            (
			            OR
			              (
				              EQUALS(VALUE("ObsoleteStatusCode"),"D"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"T"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"OTB"),
				              EQUALS(VALUE("ObsoleteStatusCode"),"D-OU"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"T-OU"), 
				              EQUALS(VALUE("ObsoleteStatusCode"),"OTBM")
			              ), 
			              EQUALS(IsonSale,"Yes"),
			              OR
				(	EQUALS(VALUE("Lead Time Type"),"Clearance"),
					EQUALS(VALUE("Lead Time Type"),"Floor Model")
				)			
			 )
		          ,				#Level1 True
		
	              		"Final Sale”
            	,					            #Level1 False 
		
			               "This item is in stock and ready to be picked up or shipped."  		#Default	for all other status codes/not on sale  and lead time types not clearance or Floor model	

		     
		)#Level1Closed
		
		
		
,						#Level0 False -------- NOT IN STOCK
           IF (OR				#Level1 IF Condition - if D/T/OTB
		       ( 
			EQUALS(VALUE("ObsoleteStatusCode"),"D"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"T"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"OTB"),
			EQUALS(VALUE("ObsoleteStatusCode"),"D-OU"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"T-OU"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"OTBM")
			
        		        )
	 	,				#Level1 - True
		
			“Out of Stock”
		
		,					#Level 1- False
    
	IF					#Level2 IF Condition - NS/FN?
	(
		OR	
		(	EQUALS(VALUE("ObsoleteStatusCode"),"NS"), 
			EQUALS(VALUE("ObsoleteStatusCode"),"FN") 
         	)
		,				#Level 2- True
      
			IF				#Level3 IF Condition - is LTT Special Order?
			(
				EQUALS(VALUE("Lead Time Type"),"Special Order")	
			,				#Level3 True
				CONCATENATE("Purchase this Special Order item now for estimated delivery ”,POdate,".") 
         		, 				#Level3 False

				IF				#Level4 I Condition - is LTT Made to Order?
				(	
					EQUALS(VALUE("Lead Time Type"),"Made to Order") # Level 4 Condition
					, 
						CONCATENATE("Purchase this Made to Order item now for estimated delivery ”,POdate,".")	# Level 4 True
					, # Level 4 False
					
					NULL
				) 				#Level 4 IF closed
			) 				#Level 3 IF closed
	
	       ,					#Level 2 FALSE
	
	IF			#Level3 IF Condition - DBA , LTT is Clearance and on sale?
	( 
		AND				
		(	EQUALS(VALUE("ObsoleteStatusCode"),"DBA"),
			EQUALS(VALUE("Lead Time Type"),"Clearance"),
			EQUALS(IsonSale,"Yes")
		)
		,#Level3 True
			"Final Sale"
		,#Level3 False
		 
		 IF (  AND	#Level4 DBA and not clearance	
			    (	EQUALS(VALUE("ObsoleteStatusCode"),"DBA"),
				    NOT(EQUALS(VALUE("Lead Time Type"),"Clearance”))
			    )	
			,     #Level4 True	
			 CONCATENATE("Purchase now for estimated delivery ",POdate,".")
			 
			 ,
			
		    IF ( #Level 5 
			      OR( EQUALS(VALUE("ObsoleteStatusCode"),"AR"),
			          EQUALS(VALUE("ObsoleteStatusCode"),"AR-W")
			        )
		        ,
		       
		 CONCATENATE("Purchase now for estimated delivery ",POdate,".") ,
			 
			"Out of Stock"	#AR-L displays this
			)	  #lev5 closed
			
			) #level 4 closed
	)	#Level3 closed

	) #L2 Close
     ) #L1 Close
  ) #L0 Close
)
